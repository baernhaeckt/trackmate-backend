using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Models;

namespace Trackmate.Backend.TrackNodes;

public interface ITrackNodeDataSource
{
    public Task<TrackNodeModel> CreateTrackNodeAsync(CreateTrackNodeModel model, CancellationToken cancellationToken);

    public Task<TrackNodeModel> AppendEmbeddingAsync(Guid trackNodeId, PictureEmbeddingModel embedding, CancellationToken cancellationToken);

    public Task<TrackNodeModel> GetTrackNodeAsync(Guid trackNodeId, CancellationToken cancellationToken);
}
