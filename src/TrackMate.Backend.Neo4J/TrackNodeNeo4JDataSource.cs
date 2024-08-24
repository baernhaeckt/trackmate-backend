using Microsoft.Extensions.Options;
using Neo4j.Driver;
using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Models;
using Trackmate.Backend.TrackNodes;

namespace TrackMate.Backend.Neo4J;

public class TrackNodeNeo4JDataSource(IOptions<TrackNodeNeo4JDataSourceSettings> settings) : ITrackNodeDataSource
{
    private const string TrackNodeLabel = "TrackNode";

    private IDriver CreateDriver()
        => CreateDriver(settings.Value);

    private static IDriver CreateDriver(TrackNodeNeo4JDataSourceSettings settings)
        => GraphDatabase.Driver(settings.HostUri, AuthTokens.Basic(settings.Username, settings.Password));

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
                Location = new double[] { model.Location.Longitude, model.Location.Latitude, model.Location.Height },
                Vector = new double[] { model.Vector.X, model.Vector.Y, model.Vector.Z },
                Orientation = new double[] { model.Orientation.Alpha, model.Orientation.Beta }
            });

        if (model.previousTrackNodeId.HasValue)
        {
            await CreateEdge(model.previousTrackNodeId.Value, trackNodeId, cancellationToken: cancellationToken);
        }

        return new TrackNodeModel(trackNodeId, model.Location, model.Vector, model.Orientation);
    }

    private async Task CreateEdge(Guid sourceNodeId, Guid targetNodeId, CancellationToken cancellationToken = default)
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

        return new TrackNodeModel(
            Guid.Parse(node["Id"].As<string>()),
            new GeoLocation(node["Location"].As<List<double>>()[1], node["Location"].As<List<double>>()[0], node["Location"].As<List<double>>()[2]),
            new TransformationVector(node["Vector"].As<List<double>>()[0], node["Vector"].As<List<double>>()[1], node["Vector"].As<List<double>>()[2]),
            new Orientation(node["Orientation"].As<List<double>>()[0], node["Orientation"].As<List<double>>()[1]));
    }
}

