using System.Collections.Concurrent;

namespace Trackmate.Backend.Tracks;

public class InMemoryTrackDataSource : ITrackDataSource
{
    private static readonly ConcurrentDictionary<string, TrackModel> _tracks = new();

    public Task<TrackModel> CreateTrackAsync(CreateTrackModel createTrackModel, CancellationToken cancellationToken)
    {
        TrackModel trackModel = new()
        {
            TrackId = Guid.NewGuid().ToString("N")[..10],
            GoalNode = createTrackModel.GoalNode,
            VisitedNodes = [new VisitedTrackNodeModel(createTrackModel.StartNode, DateTimeOffset.Now)]
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
