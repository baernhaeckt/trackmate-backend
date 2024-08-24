using System.Collections.Concurrent;
using System.Numerics;
using Trackmate.Backend.Models;

namespace Trackmate.Backend.Tracks;

public interface ITrackDataSource
{
    public Task<TrackModel> CreateTrackAsync(CreateTrackModel createTrackModel, CancellationToken cancellationToken);

    public Task<TrackModel> UpdateTrackAsync(TrackModel trackModel, CancellationToken cancellationToken);

    public Task<TrackModel> GetTrackAsync(string trackId, CancellationToken cancellationToken);
}

public class InMemoryTrackDataSource : ITrackDataSource
{
    private static readonly ConcurrentDictionary<string, TrackModel> _tracks = new();

    public Task<TrackModel> CreateTrackAsync(CreateTrackModel createTrackModel, CancellationToken cancellationToken)
    {
        TrackModel trackModel = new TrackModel()
        {
            TrackId = Guid.NewGuid().ToString("N").Substring(0, 10),
            GoalNode = createTrackModel.GoalNode,
            VisitedNodes = new List<VisitedTrackNodeModel>()
            {
                new VisitedTrackNodeModel(createTrackModel.StartNode, DateTimeOffset.Now)
            }
        };

        _tracks.AddOrUpdate(trackModel.TrackId, trackModel, (key, oldValue) => trackModel);

        return Task.FromResult(trackModel);
    }

    public Task<TrackModel> GetTrackAsync(string trackId, CancellationToken cancellationToken)
        => Task.FromResult(_tracks.GetValueOrDefault(trackId)!);

    public Task<TrackModel> UpdateTrackAsync(TrackModel trackModel, CancellationToken cancellationToken)
    {
        _tracks.AddOrUpdate(trackModel.TrackId, trackModel, (key, oldValue) => trackModel);
        return Task.FromResult(trackModel);
    }
}

public record CreateTrackModel(TrackNodeModel StartNode, TrackNodeModel GoalNode);