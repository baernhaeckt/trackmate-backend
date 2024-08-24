using FluentAssertions;
using Microsoft.Extensions.Options;
using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Models;
using TrackMate.Backend.Neo4J;
using TrackMate.Backend.Tests.Setup;

namespace TrackMate.Backend.Tests;


[Collection(ApplicationCollection.Name)]
public class TrackNodeNeo4JDataSourceFixture(ApplicationFixture applicationFixture)
{
    [Fact]
    public async Task ShouldInsertTrackNode()
    {
        // Arrange
        TrackNodeNeo4JDataSource dataSource = new TrackNodeNeo4JDataSource(Options.Create(applicationFixture.Neo4JDataSourceSettings));
        CreateTrackNodeModel model = new CreateTrackNodeModel(
            new GeoLocation(1.0, 2.0, 3.0),
            new TransformationVector(4.0, 5.0, 6.0),
            new Orientation(7.0, 8.0));

        // Act
        TrackNodeModel createdModel = await dataSource.CreateTrackNodeAsync(model, CancellationToken.None);

        // Assert
        createdModel.Should().NotBeNull();

        TrackNodeModel searchedModel = await dataSource.GetTrackNodeAsync(createdModel.Id, CancellationToken.None);

        searchedModel.Should().NotBeNull();
        searchedModel.Should().BeEquivalentTo(createdModel);
    }

    [Fact]
    public async Task ShouldInsertTrackNodeAndCreateRelation()
    {
        // Arrange
        TrackNodeNeo4JDataSource dataSource = new TrackNodeNeo4JDataSource(Options.Create(applicationFixture.Neo4JDataSourceSettings));
        CreateTrackNodeModel model = new CreateTrackNodeModel(
            new GeoLocation(0, 0, 0),
            new TransformationVector(0, 0, 0),
            new Orientation(0, 0));

        CreateTrackNodeModel model2 = new CreateTrackNodeModel(
            new GeoLocation(0.1, 0.1, 0),
            new TransformationVector(0.1, 0.1, 0),
            new Orientation(0, 0));


        // Act
        TrackNodeModel createdModel = await dataSource.CreateTrackNodeAsync(model, CancellationToken.None);
        TrackNodeModel createdModel2 = await dataSource.CreateTrackNodeAsync(model2 with { previousTrackNodeId = createdModel.Id }, CancellationToken.None);
        
        // Assert
        createdModel.Should().NotBeNull();
        createdModel2.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldAppendEmbeddingToTrackNode()
    {
        // Arrange
        TrackNodeNeo4JDataSource dataSource = new TrackNodeNeo4JDataSource(Options.Create(applicationFixture.Neo4JDataSourceSettings));
        CreateTrackNodeModel model = new CreateTrackNodeModel(
            new GeoLocation(0, 0, 0),
            new TransformationVector(0, 0, 0),
            new Orientation(0, 0));

        PictureEmbeddingModel embedding = new PictureEmbeddingModel(new float[] { 0.111F, 0.222F, 0.333F });

        // Act
        TrackNodeModel createdModel = await dataSource.CreateTrackNodeAsync(model, CancellationToken.None);
        await dataSource.AppendEmbeddingAsync(createdModel.Id, embedding, CancellationToken.None);

        // Assert
        createdModel.Should().NotBeNull();
    }
}
