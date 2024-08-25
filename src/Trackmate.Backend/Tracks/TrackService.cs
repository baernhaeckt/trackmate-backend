using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Numerics;
using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Instructions;
using Trackmate.Backend.Models;
using Trackmate.Backend.TrackNodes;

namespace Trackmate.Backend.Tracks;

public class TrackService(
    ILogger<TrackService> logger,
    IOptions<TrackServiceSettings> trackServiceSettings,
    ITrackDataSource trackDataSource, 
    ITrackNodeDataSource trackNodeDataSource,
    PictureEmbeddingClient pictureEmbeddingClient,
    InstructionsClient instructionsClient)
{
    public delegate Task AnnouncePictureDetectionResult(FoundTrackNodeModel? found);

    public async Task<TrackModel> StartTrackAsync(StartTrackModel startTrackModel)
    {
        CreateTrackModel createTrackModel = await CreateTrackModel(startTrackModel);
        return await trackDataSource.CreateTrackAsync(createTrackModel, CancellationToken.None);
    }

    private async Task<CreateTrackModel> CreateTrackModel(StartTrackModel startTrackModel)
    {
        TrackNodeModel startTrackNode = await trackNodeDataSource.GetTrackNodeAsync(startTrackModel.StartTrackNodeId, default);
        TrackNodeModel goalTrackNode = await trackNodeDataSource.GetTrackNodeAsync(startTrackModel.GoalTrackNodeId, default);

        return new CreateTrackModel(startTrackNode, goalTrackNode);
    }

    public Task<TrackModel> GetTrackAsync(string trackId)
        => trackDataSource.GetTrackAsync(trackId, CancellationToken.None);

    public async Task<TrackUpdateResult> UpdateTrackAsync(
        UploadTrackPositionPicture uploadTrackPositionPicture,
        AnnouncePictureDetectionResult announcePictureDetectionResult,
        CancellationToken cancellationToken = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        TrackModel track = await trackDataSource.GetTrackAsync(uploadTrackPositionPicture.TrackId, cancellationToken);
        track.LastPictureUploadDateTime = DateTimeOffset.Now;
        logger.LogInformation("Loaded track ({elapsedMiliseconds}ms)", stopwatch.ElapsedMilliseconds);

        stopwatch.Restart();
        PictureEmbeddingModel model = await pictureEmbeddingClient.GeneratePictureEmbeddingAsync(uploadTrackPositionPicture.MimeType, uploadTrackPositionPicture.PictureBase64);
        logger.LogInformation("Generated picture embedding ({elapsedMiliseconds}ms)", stopwatch.ElapsedMilliseconds);

        stopwatch.Restart();
        FoundTrackNodeModel foundTrackNodeModel = await trackNodeDataSource.FindByEmbeddingAndDistance(model, track.LastVisitedNode.TrackNode.Id, CancellationToken.None);
        logger.LogInformation("Find track node ({elapsedMiliseconds}ms)", stopwatch.ElapsedMilliseconds);

        if (foundTrackNodeModel == FoundTrackNodeModel.None)
        {
            await announcePictureDetectionResult(null);
            logger.LogInformation("No track node found for embedding.");
            return TrackUpdateResult.NoLocation;
        }
        await announcePictureDetectionResult(foundTrackNodeModel);

        stopwatch.Restart();
        TrackNodeModel currentTrackNode = await trackNodeDataSource.GetTrackNodeAsync(track.LastVisitedNode.TrackNode.Id, CancellationToken.None);
        TrackNodePath path = await trackNodeDataSource.FindPathAsync(foundTrackNodeModel.TrackNodeId, track.GoalNode.Id, CancellationToken.None);
        TrackNodeModel nextTrackNodeModel = await FindNextRelevantTrackNodeModel(path);
        logger.LogInformation("Path found ({elapsedMiliseconds}ms)", stopwatch.ElapsedMilliseconds);

        if (IsInstructionNecessary(track, nextTrackNodeModel)
            || track.LastHintDateTime + trackServiceSettings.Value.InstructionTimeout < DateTimeOffset.Now)
        {
            logger.LogInformation("A instruction is generated, either because necessary or timeout reached.");
            track.VisitedNodes.Add(new VisitedTrackNodeModel(currentTrackNode, DateTimeOffset.Now));
            track.LastHintDateTime = DateTimeOffset.Now;

            stopwatch.Restart();
            InstructionRequestModel instructionRequestModel = new(
                track.LastVisitedNode.TrackNode.Location,
                nextTrackNodeModel.Location);

            string instructionText = await instructionsClient.CreateInstructionTextAsync(instructionRequestModel);
            Stream audioStream = await instructionsClient.CreateInstructionAudioAsync(instructionRequestModel);
            logger.LogInformation("Instruction generated ({elapsedMiliseconds}ms)", stopwatch.ElapsedMilliseconds);

            return TrackUpdateResult.NewInstruction(instructionText, audioStream);
        }

        track.VisitedNodes.Add(new VisitedTrackNodeModel(currentTrackNode, DateTimeOffset.Now));
        return TrackUpdateResult.LocationUpdated;
    }

    /// <summary>
    ///     We currently take the next <see cref="TrackNodeModel"> from the path to generate an instruction. <br />
    ///     TODO: A better approach would be to take the next <see cref="TrackNodeModel"> before a big angle change. 
    /// </summary>
    private static async Task<TrackNodeModel> FindNextRelevantTrackNodeModel(TrackNodePath path)
    {
        await Task.CompletedTask;
        return path.Nodes[1];
    }

    /// <summary>
    ///     Checks if with the current <see cref="TrackModel"/> and its <see cref="TrackModel.CurrentVector"/> an instruction is necessary
    ///     when going to the next <see cref="TrackNodeModel"/>.
    /// </summary>
    private bool IsInstructionNecessary(TrackModel track, TrackNodeModel nextTrackNodeModel)
    {
        // disable for DEMO
        return false;

        Vector3 currentVector = track.CurrentVector;
        Vector3 nextVector = nextTrackNodeModel.Location.AsCoordinates();
        double dotProduct = currentVector.X * nextVector.X + currentVector.Y * nextVector.Y + currentVector.Z * nextVector.Z;

        // Calculate magnitudes of A and B
        double magnitudeA = Math.Sqrt(currentVector.X * currentVector.X + currentVector.Y * currentVector.Y + currentVector.Z * currentVector.Z);
        double magnitudeB = Math.Sqrt(nextVector.X * nextVector.X + nextVector.Y * nextVector.Y + nextVector.Z * nextVector.Z);

        // Calculate the angle in radians
        double angleRadians = Math.Acos(dotProduct / (magnitudeA * magnitudeB));

        // Convert to degrees if needed
        double angleDegrees = angleRadians * (180.0 / Math.PI);

        return angleDegrees > trackServiceSettings.Value.AngleThresholdInDegrees;
    }
}