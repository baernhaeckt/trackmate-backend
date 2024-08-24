using Microsoft.Extensions.Options;
using Neo4j.Driver;
using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Models;
using Trackmate.Backend.TrackNodes;

namespace TrackMate.Backend.Neo4J;

public class TrackNodeNeo4JDataSource(IOptions<TrackNodeNeo4JDataSourceSettings> settings) : ITrackNodeDataSource
{
    private IDriver CreateDriver()
        => CreateDriver(settings.Value);

    private static IDriver CreateDriver(TrackNodeNeo4JDataSourceSettings settings)
        => GraphDatabase.Driver(settings.HostUri, AuthTokens.Basic(settings.Username, settings.Password));

    public Task<TrackNodeModel> CreateTrackNodeAsync(CreateTrackNodeModel model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TrackNodeModel> AppendEmbeddingAsync(Guid trackNodeId, PictureEmbeddingModel embedding, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

