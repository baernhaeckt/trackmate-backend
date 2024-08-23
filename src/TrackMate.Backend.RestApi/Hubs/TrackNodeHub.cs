using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Trackmate.Backend;
using Trackmate.Backend.Models;

namespace TrackMate.Backend.RestApi.Hubs;

public class TrackNodeHub(ILogger<TrackNodeHub> logger, TrackNodeService trackNodeService) : Hub
{
    private static readonly ConcurrentDictionary<string, List<ISingleClientProxy>> _trackSubscribers = new();

    private static readonly Dictionary<Guid, Stream> _trackNodeUploadDictionary = new();

    private static readonly Dictionary<string, Stream> _trackPictureUploadDictionary = new();

    public async Task CreateTrackNode(CreateTrackNodeModel model)
    {
        TrackNodeModel CreateTrackNodeModel = await trackNodeService.CreateTrackNode(model);
        _trackNodeUploadDictionary[CreateTrackNodeModel.Id] = new MemoryStream();

        await Clients.Caller.SendAsync("TrackNodeCreated", CreateTrackNodeModel);
    }

    public async Task UploadPictureChunkForTrackNode(Guid trackNodeId, string mimeType, byte[] chunk, bool isLastChunk)
    {
        logger.LogInformation("Uploaded chunk({byteSize}) for new track node {trackNodeId}.", chunk.Length, trackNodeId);
        await _trackNodeUploadDictionary[trackNodeId].WriteAsync(chunk);

        if (isLastChunk)
        {
            logger.LogInformation("Uploaded last chunk for track node {trackNodeId}, MimeType: {mimeType}.", trackNodeId, mimeType);
            await Clients.Caller.SendAsync("Uploaded ", trackNodeId);

            Stream stream = _trackNodeUploadDictionary[trackNodeId];
            _trackNodeUploadDictionary.Remove(trackNodeId);
            stream.Seek(0, SeekOrigin.Begin);

            await trackNodeService.UploadPicture(new UploadPictureModel(trackNodeId, mimeType, stream));
        }
    }

    public async Task StartTrack(StartTrackModel startTrackModel)
    {
        string trackId = Guid.NewGuid().ToString("N").Substring(0, 8);

        _trackSubscribers.AddOrUpdate(
            trackId, 
            new List<ISingleClientProxy> { Clients.Caller },
            (_, list) => 
            {
                list.Add(Clients.Caller);
                return list;
            });

        await Clients.Caller.SendAsync("TrackStarted", trackId);
    }

    public async Task JoinTrack(string trackId)
    {
        _trackSubscribers[trackId].Add(Clients.Caller);
        await SendToTrackAsync(trackId, "UserJoined");
    }

    public async Task CompleteTrack(string trackId)
    {
        await SendToTrackAsync(trackId, "TrackCompleted");
        _trackSubscribers.TryRemove(trackId, out _);
    }

    public async Task UploadTrackPositionPicture(string trackId, string mimeType, byte[] chunk, bool isLastChunk)
    {
        logger.LogInformation("Uploaded chunk({byteSize}) for track position {trackId}.", chunk.Length, trackId);

        await _trackPictureUploadDictionary[trackId].WriteAsync(chunk);

        if (isLastChunk)
        {
            logger.LogInformation("Uploaded last chunk for track {trackId} to find position, MimeType: {mimeType}.", trackId, mimeType);
            await Clients.Caller.SendAsync("TrackPositionPictureUploaded", trackId);

            Stream stream = _trackPictureUploadDictionary[trackId];
            _trackPictureUploadDictionary.Remove(trackId);
            stream.Seek(0, SeekOrigin.Begin);

            // await trackNodeService.UploadPicture(new UploadPictureModel(trackNodeId, mimeType, stream));
        }
    }

    private Task SendToTrackAsync(string trackId, string methodName, object? arg1 = null)
        => Task.WhenAll(_trackSubscribers[trackId].Select(client => client.SendAsync(methodName, arg1)));
}
