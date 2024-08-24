using Trackmate.Backend.Models;
using Neo4j.Driver;

namespace Trackmate.Backend.TrackNodes;

public static class INodeExtensions
{
    public static TrackNodeModel Map(this INode trackNode)
        => new TrackNodeModel(
            Guid.Parse(trackNode["Id"].As<string>()),
            trackNode.MapLocation(),
            trackNode.MapVector(),
            trackNode.MapOrientation());

    public static GeoLocation MapLocation(this INode trackNode)
    {
        List<double> locationParams = trackNode["Location"].As<List<double>>();
        return new GeoLocation(locationParams[0], locationParams[1], locationParams[2]);
    }

    public static TransformationVector MapVector(this INode trackNode)
    {
        List<double> vectorParams = trackNode["Vector"].As<List<double>>();
        return new TransformationVector(vectorParams[0], vectorParams[1], vectorParams[2]);
    }

    public static Orientation MapOrientation(this INode trackNode)
    {
        List<double> orientationParams = trackNode["Orientation"].As<List<double>>();
        return new Orientation(orientationParams[0], orientationParams[1]);
    }
}
