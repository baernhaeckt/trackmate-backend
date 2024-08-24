using Trackmate.Backend.Models;

namespace Trackmate.Backend.TrackNodes;

public record TrackNodePath(IReadOnlyList<TrackNodeModel> Nodes);