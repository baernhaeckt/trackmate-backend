using Trackmate.Backend.Models;

namespace Trackmate.Backend.Tracks;

public class TrackModel()
{
    public string TrackId { get; set; }

    public TrackNodeModel GoalNode { get; set; }

    public DateTimeOffset LastPictureUploadDateTime { get; set; }

    public DateTimeOffset LastHintDateTime { get; set; }

    public List<VisitedTrackNodeModel> VisitedNodes { get; set; } = new List<VisitedTrackNodeModel>();

    public VisitedTrackNodeModel LastVisitedNode
        => VisitedNodes.OrderByDescending(n => n.VisitDateTime).FirstOrDefault();
}
