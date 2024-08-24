using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Models;
using Trackmate.Backend.TrackNodes;

namespace TrackMate.Backend.Neo4J;

public class TrackNodeNeo4JDataSource(
    ILogger<TrackNodeNeo4JDataSource> logger,
    IOptions<TrackNodeNeo4JDataSourceSettings> settings) : ITrackNodeDataSource
{
    private const string TrackNodeLabel = "TrackNode";

    public async Task<TrackNodeModel> CreateTrackNodeAsync(CreateTrackNodeModel model, CancellationToken cancellationToken)
    {
        Guid trackNodeId = Guid.NewGuid();

        using IDriver driver = CreateDriver();
        using IAsyncSession session = driver.AsyncSession();

        string query = @"
                    CREATE (node:TrackNode { Id: $Id, Location: $Location, Vector: $Vector, Orientation: $Orientation })
                    RETURN node";

        IResultCursor result = await session.RunAsync(
            query,
            new
            {
                Id = trackNodeId.ToString("N"),
                Location = new double[] { model.Location.Latitude, model.Location.Longitude, model.Location.Altitude },
                Vector = new double[] { model.Vector.X, model.Vector.Y, model.Vector.Z },
                Orientation = new double[] { model.Orientation.Alpha, model.Orientation.Beta }
            });

        if (model.previousTrackNodeId.HasValue)
        {
            await CreateEdgeAsync(model.previousTrackNodeId.Value, trackNodeId, cancellationToken: cancellationToken);
        }

        return new TrackNodeModel(trackNodeId, model.Location, model.Vector, model.Orientation);
    }

    public async Task<TrackNodeModel> AppendEmbeddingAsync(Guid trackNodeId, PictureEmbeddingModel embedding, CancellationToken cancellationToken)
    {
        using IDriver driver = CreateDriver();
        using IAsyncSession session = driver.AsyncSession();

        string query = @"
                    MATCH (node:TrackNode {Id: $TrackNodeId})
                    SET node.embedding = $Embedding";

        IResultCursor result = await session.RunAsync(
            query,
            new { TrackNodeId = trackNodeId.ToString("N"), embedding.Embedding });

        return await GetTrackNodeAsync(trackNodeId, cancellationToken);
    }

    public async Task<TrackNodeModel> GetTrackNodeAsync(Guid trackNodeId, CancellationToken cancellationToken)
    {
        using IDriver driver = CreateDriver();
        using IAsyncSession session = driver.AsyncSession();

        string query = @"
                    MATCH (node:TrackNode {Id: $TrackNodeId})
                    RETURN node";

        IResultCursor result = await session.RunAsync(
            query,
            new { TrackNodeId = trackNodeId.ToString("N") });

        IRecord record = await result.SingleAsync();
        INode node = record["node"].As<INode>();

        return node.Map();
    }

    public async Task<FoundTrackNodeModel> FindByEmbeddingAndDistance(PictureEmbeddingModel embedding, Guid trackNodeId, CancellationToken cancellationToken)
    {
        using IDriver driver = CreateDriver();
        using IAsyncSession session = driver.AsyncSession();

        string query = @"
                    WITH $Embedding AS search_vector
                    MATCH (node:TrackNode)
                    WHERE gds.similarity.cosine(node.embedding, search_vector) > 0.5 
                    RETURN node, gds.similarity.cosine(node.embedding, search_vector) AS similarity
                    ORDER BY similarity DESC";

        IResultCursor result = await session.RunAsync(
            query,
            new { TrackNodeId = trackNodeId.ToString("N"), embedding.Embedding });

        IRecord? record = (await result.ToListAsync()).FirstOrDefault();

        if (record == null)
        {
            return FoundTrackNodeModel.None;
        }

        INode node = record["node"].As<INode>();

        if (node == null)
        {
            return FoundTrackNodeModel.None;
        }

        logger.LogInformation("Found track node {TrackNodeId} with similarity {Similarity} and distance {Distance}.", 
            node["Id"].As<string>(), 
            record["similarity"].As<double>(), 
            0);

        return new FoundTrackNodeModel(
            Guid.Parse(node["Id"].As<string>()),
            Similarity: record["similarity"].As<double>(),
            Distance: 0);
    }

    public async Task<TrackNodePath> FindPathAsync(Guid sourceNodeId, Guid targetNodeId, CancellationToken cancellationToken)
    {
        using IDriver driver = CreateDriver();
        using IAsyncSession session = driver.AsyncSession();

        string query = @"
                    MATCH path = (source:TrackNode { Id: $SourceId })-[:PATH*]->(target:TrackNode { Id: $TargetId })
                    RETURN nodes(path) as nodes";

        IResultCursor result = await session.RunAsync(
            query,
            new
            {
                SourceId = sourceNodeId.ToString("N"),
                TargetId = targetNodeId.ToString("N")
            });

        IRecord record = await result.SingleAsync();
        List<INode> nodes = record["nodes"].As<List<INode>>();

        return new TrackNodePath(nodes.Select(node => node.Map()).ToList());
    }

    public async Task CreateEdgeAsync(Guid sourceNodeId, Guid targetNodeId, CancellationToken cancellationToken = default)
    {
        using IDriver driver = CreateDriver();
        using IAsyncSession session = driver.AsyncSession();

        string query = @"
                    MATCH (source:TrackNode { Id: $SourceId })
                    MATCH (target:TrackNode { Id: $TargetId })
                    CREATE (source)-[:PATH]->(target)";

        await session.RunAsync(query, new
        {
            SourceId = sourceNodeId.ToString("N"),
            TargetId = targetNodeId.ToString("N")
        });
    }

    private IDriver CreateDriver()
        => CreateDriver(settings.Value);

    private static IDriver CreateDriver(TrackNodeNeo4JDataSourceSettings settings)
        => GraphDatabase.Driver(settings.Uri, AuthTokens.Basic(settings.Username, settings.Password));
}

