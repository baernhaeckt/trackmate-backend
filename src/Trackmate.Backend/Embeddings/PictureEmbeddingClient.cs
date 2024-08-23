using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Trackmate.Backend.Embeddings;

public class PictureEmbeddingClient(IOptions<PictureEmbeddingClientSettings> settings, Func<HttpClientHandler, HttpClient> httpClientBuilder)
{
    private HttpClient? _httpClient;

    public HttpClient HttpClient
        => _httpClient ??= httpClientBuilder(new HttpClientHandler());

    private HttpClient CreateHttpClient()
    {
        HttpClient newClient = httpClientBuilder(new HttpClientHandler());
        HttpClient.BaseAddress = settings.Value.BaseUri;

        return newClient;
    }

    public async Task<PictureEmbeddingModel> GeneratePictureEmbedding(string mimeType, Stream pictureDataStream)
    {
        const string uri = "api/v1/embedding/create";

        HttpResponseMessage response = await HttpClient.PostAsync(
            new Uri(uri, UriKind.Relative),
            new MultipartFormDataContent
            {
                { new StreamContent(pictureDataStream), "file", "file" },
                { new StringContent(mimeType), "mimeType" }
            });

        return await response.Content.ReadFromJsonAsync<PictureEmbeddingModel>()!;
    }
}