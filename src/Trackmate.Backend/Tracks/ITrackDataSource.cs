namespace Trackmate.Backend.Tracks;

public interface ITrackDataSource
{
}

public class InMemoryTrackDataSource : ITrackDataSource
{
    
    public Task CreateTrack(TrackModel track, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TrackModel> GetTrack(Guid trackId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}