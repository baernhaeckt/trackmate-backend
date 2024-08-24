using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Models;
using Trackmate.Backend.TrackNodes;
using TrackMate.Backend.Neo4J;
using TrackMate.Backend.Tests.Builders;
using TrackMate.Backend.Tests.Setup;

namespace TrackMate.Backend.Tests;


[Collection(ApplicationCollection.Name)]
public class TrackNodeNeo4JDataSourceFixture(ApplicationFixture applicationFixture)
{
    ILogger<TrackNodeNeo4JDataSource> logger = new NullLogger<TrackNodeNeo4JDataSource>();

    [Fact]
    public async Task CreateTrackNodeAsync_ShouldInsertTrackNode()
    {
        // Arrange
        TrackNodeNeo4JDataSource dataSource = new TrackNodeNeo4JDataSource(logger, Options.Create(applicationFixture.Neo4JDataSourceSettings));
        CreateTrackNodeModel model = CreateTrackNodeModelBuilder.Create().Build();

        // Act
        TrackNodeModel createdModel = await dataSource.CreateTrackNodeAsync(model, CancellationToken.None);

        // Assert
        createdModel.Should().NotBeNull();

        TrackNodeModel searchedModel = await dataSource.GetTrackNodeAsync(createdModel.Id, CancellationToken.None);

        searchedModel.Should().NotBeNull();
        searchedModel.Should().BeEquivalentTo(createdModel);
    }

    [Fact]
    public async Task CreateTrackNodeAsync_ShouldInsertTrackNodeAndCreateRelation()
    {
        // Arrange
        TrackNodeNeo4JDataSource dataSource = new TrackNodeNeo4JDataSource(logger, Options.Create(applicationFixture.Neo4JDataSourceSettings));
        CreateTrackNodeModel model = CreateTrackNodeModelBuilder.Create().Build();
        TrackNodeModel createdModel = await dataSource.CreateTrackNodeAsync(model, CancellationToken.None);

        CreateTrackNodeModel model2 = CreateTrackNodeModelBuilder.Create().WithGeoLocation(2.0, 3.0, 4.0).Build();

        // Act
        TrackNodeModel createdModel2 = await dataSource.CreateTrackNodeAsync(model2 with { previousTrackNodeId = createdModel.Id }, CancellationToken.None);

        // Assert
        createdModel.Should().NotBeNull();
        createdModel2.Should().NotBeNull();
    }

    [Fact]
    public async Task AppendEmbeddingAsync_ShouldAppendEmbeddingToTrackNode()
    {
        // Arrange
        TrackNodeNeo4JDataSource dataSource = new TrackNodeNeo4JDataSource(logger, Options.Create(applicationFixture.Neo4JDataSourceSettings));
        CreateTrackNodeModel model = CreateTrackNodeModelBuilder.Create().Build();
        TrackNodeModel createdModel = await dataSource.CreateTrackNodeAsync(model, CancellationToken.None);

        PictureEmbeddingModel embedding = new PictureEmbeddingModel(new float[] { 0.111F, 0.222F, 0.333F });

        // Act
        await dataSource.AppendEmbeddingAsync(createdModel.Id, embedding, CancellationToken.None);

        // Assert
        createdModel.Should().NotBeNull();
    }

    [Fact]
    public async Task FindPathAsync_ShouldFindBestPath()
    {
        // Arrange
        TrackNodeNeo4JDataSource dataSource = new TrackNodeNeo4JDataSource(logger, Options.Create(applicationFixture.Neo4JDataSourceSettings));
        List<TrackNodeModel> nodes = await CreateManyNodesList(dataSource);

        // Create Circle Edges
        await Task.WhenAll(
            dataSource.CreateEdgeAsync(nodes[0].Id, nodes[1].Id, CancellationToken.None),
            dataSource.CreateEdgeAsync(nodes[1].Id, nodes[2].Id, CancellationToken.None),
            dataSource.CreateEdgeAsync(nodes[3].Id, nodes[4].Id, CancellationToken.None),
            dataSource.CreateEdgeAsync(nodes[4].Id, nodes[5].Id, CancellationToken.None),
            dataSource.CreateEdgeAsync(nodes[0].Id, nodes[9].Id, CancellationToken.None),
            dataSource.CreateEdgeAsync(nodes[9].Id, nodes[8].Id, CancellationToken.None),
            dataSource.CreateEdgeAsync(nodes[7].Id, nodes[6].Id, CancellationToken.None),
            dataSource.CreateEdgeAsync(nodes[6].Id, nodes[5].Id, CancellationToken.None));

        // Create Short Cut
        await dataSource.CreateEdgeAsync(nodes[2].Id, nodes[5].Id, CancellationToken.None);

        // Act
        TrackNodePath path = await dataSource.FindPathAsync(nodes[0].Id, nodes[5].Id, CancellationToken.None);

        // Assert
        path.Should().NotBeNull();
        path.Nodes.Should().HaveCount(4);
        path.Nodes[0].Should().BeEquivalentTo(nodes[0]);
        path.Nodes[1].Should().BeEquivalentTo(nodes[1]);
        path.Nodes[2].Should().BeEquivalentTo(nodes[2]);
        path.Nodes[3].Should().BeEquivalentTo(nodes[5]);
    }

    [Fact]
    public async Task FindByEmbeddingAndDistance_ShouldFindMatch()
    {
        // Arrange
        TrackNodeNeo4JDataSource dataSource = new TrackNodeNeo4JDataSource(logger, Options.Create(applicationFixture.Neo4JDataSourceSettings));
        List<TrackNodeModel> nodes = await CreateManyNodesList(dataSource, 4);

        await Task.WhenAll(
            dataSource.AppendEmbeddingAsync(nodes[0].Id, new PictureEmbeddingModel(new float[] { 0.011F, 0.011F, 0.011F }), CancellationToken.None),
            dataSource.AppendEmbeddingAsync(nodes[1].Id, new PictureEmbeddingModel(new float[] { 0.110F, 0.101F, 0.111F }), CancellationToken.None), 
            dataSource.AppendEmbeddingAsync(nodes[2].Id, new PictureEmbeddingModel(new float[] { 0.011F, 0.011F, 0.011F }), CancellationToken.None),
            dataSource.AppendEmbeddingAsync(nodes[3].Id, new PictureEmbeddingModel(new float[] { 0.011F, 0.011F, 0.011F }), CancellationToken.None));
        
        await Task.WhenAll(
            dataSource.CreateEdgeAsync(nodes[0].Id, nodes[1].Id, CancellationToken.None),
            dataSource.CreateEdgeAsync(nodes[0].Id, nodes[2].Id, CancellationToken.None),
            dataSource.CreateEdgeAsync(nodes[0].Id, nodes[3].Id, CancellationToken.None));

        // Act
        FoundTrackNodeModel foundTrackNodeModel = await dataSource.FindByEmbeddingAndDistance(
            new PictureEmbeddingModel(new float[] { 0.111F, 0.111F, 0.111F }), 
            nodes[0].Id, 
            CancellationToken.None);

        // Assert
        foundTrackNodeModel.Should().NotBeNull();
        foundTrackNodeModel.Similarity.Should().BeGreaterThan(0.95);
        foundTrackNodeModel.TrackNodeId.Should().Be(nodes[1].Id);
        foundTrackNodeModel.Distance.Should().Be(1);
    }

    [Fact]
    public async Task FindByEmbeddingAndDistance_ShouldMatchWithImages()
    {
        // Arrange
        TrackNodeNeo4JDataSource dataSource = new TrackNodeNeo4JDataSource(logger, Options.Create(applicationFixture.Neo4JDataSourceSettings));
        IOptions<PictureEmbeddingClientSettings> settings = Options.Create(new PictureEmbeddingClientSettings()
        {
            BaseUri = new Uri("https://trackmate-embedding-cbfje4ebcfgsfaay.westeurope-01.azurewebsites.net/")
        });

        using PictureEmbeddingClient client = new PictureEmbeddingClient(settings);
        Guid? lastNodeId = null;

        Dictionary<Guid, PictureEmbeddingModel> embeddings = new Dictionary<Guid, PictureEmbeddingModel>();

        for (int i = 1; i < 9; i++)
        {
            using Stream fileStream = File.OpenRead($"Data/set1image{i}.jpeg");
            PictureEmbeddingModel embedding = await client.GeneratePictureEmbeddingAsync("image/jpeg", fileStream);
            TrackNodeModel trackNode = await dataSource.CreateTrackNodeAsync(CreateTrackNodeModelBuilder
                .Create()
                .WithPreviousTrackNodeId(lastNodeId)
                .Build(), CancellationToken.None);
            lastNodeId = trackNode.Id;

            embeddings.Add(trackNode.Id, embedding);
            await dataSource.AppendEmbeddingAsync(trackNode.Id, embedding, CancellationToken.None);
        }

        // Act
        FoundTrackNodeModel foundTrackNodeModel = await dataSource.FindByEmbeddingAndDistance(
            embeddings[embeddings.Keys.Last()],
            embeddings.Keys.First(),
            CancellationToken.None);

        // Assert

        foundTrackNodeModel.Should().NotBeNull();
    }


    private static async IAsyncEnumerable<TrackNodeModel> CreateManyNodes(TrackNodeNeo4JDataSource dataSource, int count = 10)
    {
        for (int i = 0; i < count; i++)
        {
            yield return await dataSource.CreateTrackNodeAsync(CreateTrackNodeModelBuilder.Create().Build(), CancellationToken.None);
        }
    }

    private static async Task<List<TrackNodeModel>> CreateManyNodesList(TrackNodeNeo4JDataSource dataSource, int count = 10)
    {
        List<TrackNodeModel> result = new List<TrackNodeModel>(count);
        await foreach (TrackNodeModel model in CreateManyNodes(dataSource, count))
        {
            result.Add(model);
        }

        return result;
    }
}
