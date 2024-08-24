using Trackmate.Backend.Models;

namespace Trackmate.Backend.Tracks;

public record VisitedTrackNodeModel(TrackNodeModel TrackNode, DateTimeOffset VisitDateTime);