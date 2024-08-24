namespace Trackmate.Backend.TrackNodes;

public record FoundTrackNodeModel(Guid TrackNodeId, double Similarity, double Distance)
{
    public static FoundTrackNodeModel None
        => new FoundTrackNodeModel(Guid.Empty, 0.0, double.MaxValue);
}
