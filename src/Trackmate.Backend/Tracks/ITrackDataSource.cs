using System.Numerics;

namespace Trackmate.Backend.Tracks;

public interface ITrackDataSource
{
    public Task<TrackModel> CreateTrackAsync(CreateTrackModel createTrackModel, CancellationToken cancellationToken);

    public Task<TrackModel> UpdateTrackAsync(TrackModel trackModel, CancellationToken cancellationToken);

    public Task<TrackModel> GetTrackAsync(string trackId, CancellationToken cancellationToken);
}
