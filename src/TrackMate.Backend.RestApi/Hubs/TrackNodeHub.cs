﻿using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text;
using Trackmate.Backend;
using Trackmate.Backend.Models;
using Trackmate.Backend.TrackNodes;
using Trackmate.Backend.Tracks;

namespace TrackMate.Backend.RestApi.Hubs;

public class TrackNodeHub(ILogger<TrackNodeHub> logger, TrackNodeService trackNodeService, TrackService trackService) : Hub
{
    private static readonly ConcurrentDictionary<string, List<ISingleClientProxy>> _trackSubscribers = new();

    /// <summary>
    ///     Creates a new <see cref="TrackNodeModel"/> to be used to create tracks.
    ///     This method does not complete the node, as the picture is uploaded in chunks with the method <see cref="UploadPictureChunkForTrackNode"/>.
    /// </summary>
    public async Task<TrackNodeModel> CreateTrackNode(CreateTrackNodeModel model)
    {
        return await trackNodeService.CreateTrackNodeAsync(model, default);
    }

    /// <summary>
    ///    Uploads a chunk of a picture for a track node.
    /// </summary>
    /// <param name="trackNodeId">Id of the <see cref="TrackNodeModel"/> generated by <see cref="CreateTrackNode"/>.</param>
    /// <param name="mimeType">Mime/Type of the uploaded image.</param>
    /// <param name="chunk">Byte chunk of the image uploaded.</param>
    /// <param name="isLastChunk">Flag if the upload is completed.</param>
    public async Task UploadPictureChunkForTrackNode(UploadPictureModel uploadPictureModel)
    {
        await trackNodeService.UploadTrackNodePictureAsync(uploadPictureModel, default);
    }

    /// <summary>
    ///     Create a new track to a defined goal <see cref="TrackNodeModel"/>. <br />
    ///     A track id is created and returned to the caller, which can be used for others to join the track.
    /// </summary>
    /// <param name="startTrackModel">Information to create a track.</param>
    public async Task<string> StartTrack(StartTrackModel startTrackModel)
    {
        logger.LogInformation("Starting track from:{startTrackNodeId} to:{goalTrackNodeId}.", startTrackModel.StartTrackNodeId, startTrackModel.GoalTrackNodeId);
        await Task.CompletedTask;
        TrackModel track = await trackService.StartTrackAsync(startTrackModel);

        _trackSubscribers.AddOrUpdate(
            track.TrackId, 
            [ Clients.Caller ],
            (_, list) => 
            {
                list.Add(Clients.Caller);
                return list;
            });

        return track.TrackId;
    }

    /// <summary>
    ///    Gets all running tracks.
    /// </summary>
    public async Task<string[]> GetRunningTracks()
    {
        logger.LogInformation("Getting running tracks.");
        return _trackSubscribers.Keys.ToArray();
    }

    /// <summary>
    ///   Gets all track nodes.
    /// </summary>
    public async Task<TrackNodeModel[]> GetAllTrackNodes()
    {
        logger.LogInformation("Getting all track nodes.");
	    return await trackNodeService.GetAllTrackNodesAsync(default);
    }

    /// <summary>
    ///     Joins a ongoing track, subscribing to the updates of the track progress.
    /// </summary>
    /// <param name="trackId">Id of the track to join.</param>
    public async Task JoinTrack(string trackId)
    {
        logger.LogInformation("User joined track {trackId}.", trackId);
        _trackSubscribers[trackId].Add(Clients.Caller);
        await SendToTrackAsync(trackId, "UserJoined");
    }

    /// <summary>
    ///    Leaves a track, unsubscribing from the updates of the track progress.
    /// </summary>
    /// <param name="trackId">The track to leave.</param>
    public async Task LeaveTrack(string trackId)
	{
		logger.LogInformation("User left track {trackId}.", trackId);
		_trackSubscribers[trackId].Remove(Clients.Caller);
		await SendToTrackAsync(trackId, "UserLeft");
	}

    /// <summary>
    ///     Completes a track, notifying all subscribers that the track is completed.
    /// </summary>
    /// <param name="trackId">Id of the track to complete.</param>
    public async Task CompleteTrack(string trackId)
    {
        logger.LogInformation("Track {trackId} completed.", trackId);
        await SendToTrackAsync(trackId, "TrackCompleted");
        _trackSubscribers.TryRemove(trackId, out _);
    }

    public async Task UploadTrackPositionPicture(UploadTrackPositionPicture uploadTrackPositionPicture)
    {
        if (string.IsNullOrEmpty(uploadTrackPositionPicture.TrackId))
        {
            logger.LogWarning("API VALIDATION");
            return;
        }

        logger.LogInformation("Uploaded picture for track position {trackId}.", uploadTrackPositionPicture.TrackId);

        Task announce(FoundTrackNodeModel? foundModel) => SendToTrackAsync(uploadTrackPositionPicture.TrackId, "TrackPositionPictureMatched", foundModel);
        TrackUpdateResult result = await trackService.UpdateTrackAsync(uploadTrackPositionPicture, announce, default);

        if (result.type == TrackUpdateResultType.NewInstruction)
        {
            await Clients.Caller.SendAsync("InstructionAudio", Convert.ToBase64String(ReadAllBytesFromStream(result.instructionAudio!)));
            await SendToTrackAsync(uploadTrackPositionPicture.TrackId, "InstructionText", result.instruction);
        }
    }

    private static byte[] ReadAllBytesFromStream(Stream stream)
    {
        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static Task SendToTrackAsync(string trackId, string methodName, object? arg1 = null)
        => Task.WhenAll(_trackSubscribers[trackId].Select(client => client.SendAsync(methodName, arg1)));
}
