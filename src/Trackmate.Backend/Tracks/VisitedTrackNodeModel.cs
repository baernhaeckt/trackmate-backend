namespace Trackmate.Backend.Tracks;

public class VisitedTrackNodeModel()
{
    public Guid TrackNodeId { get; set; }

    public DateTimeOffset VisitDateTime { get; set; }
}