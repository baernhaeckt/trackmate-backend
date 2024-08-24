using System.Numerics;
using Trackmate.Backend.Models;

namespace Trackmate.Backend.Tracks;

public class TrackModel()
{
    public required string TrackId { get; set; }

    public required TrackNodeModel GoalNode { get; set; }

    public DateTimeOffset LastPictureUploadDateTime { get; set; }

    public DateTimeOffset LastHintDateTime { get; set; }

    public Vector3 CurrentVector
    {
        get
        {
            if (VisitedNodes.Count > 1)
            {
                return VisitedNodes[^1].TrackNode.Location.GetVector(VisitedNodes[^2].TrackNode.Location);
            }

            return Vector3.Zero;
        }
    }

    public List<VisitedTrackNodeModel> VisitedNodes { get; init; } = [];

    public VisitedTrackNodeModel LastVisitedNode
        => VisitedNodes.OrderByDescending(n => n.VisitDateTime).First();

}
