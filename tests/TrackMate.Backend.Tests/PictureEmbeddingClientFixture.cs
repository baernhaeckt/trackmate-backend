using FluentAssertions;
using Microsoft.Extensions.Options;
using Trackmate.Backend.Embeddings;

namespace TrackMate.Backend.Tests;

public class PictureEmbeddingClientFixture
{
    [Theory]
    [InlineData("image1.jpeg")]
    [InlineData("image2.jpeg")]
    public async Task ShouldUploadPicture(string imageName)
    {
        // Arrange
        IOptions<PictureEmbeddingClientSettings> settings = Options.Create(new PictureEmbeddingClientSettings()
        {
            BaseUri = new Uri("https://trackmate-embedding-cbfje4ebcfgsfaay.westeurope-01.azurewebsites.net/")
        });
        using Stream fileStream = File.OpenRead($"Data/{imageName}");
        using PictureEmbeddingClient client = new PictureEmbeddingClient(settings);

        // Act
        PictureEmbeddingModel result = await client.GeneratePictureEmbedding("image/jpeg", fileStream);

        // Assert
        result.Should().NotBeNull();
        result.Embedding.Should().NotBeNullOrEmpty();
    }
}