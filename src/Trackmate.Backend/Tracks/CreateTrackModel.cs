using Trackmate.Backend.Models;

namespace Trackmate.Backend.Tracks;

public record CreateTrackModel(TrackNodeModel StartNode, TrackNodeModel GoalNode);