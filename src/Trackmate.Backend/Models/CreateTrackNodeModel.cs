namespace Trackmate.Backend.Models;

public record CreateTrackNodeModel(GeoLocation Location, TransformationVector Vector, Orientation Orientation, Guid? previousTrackNodeId = null);
