using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Trackmate.Backend.Embeddings;

public sealed class PictureEmbeddingClient(
    IOptions<PictureEmbeddingClientSettings> settings, 
    Func<HttpClientHandler, HttpClient>? httpClientBuilder = null)
    : IDisposable
{
    private HttpClient? _httpClient;

    public HttpClient HttpClient
        => _httpClient ??= CreateHttpClient();

    private HttpClient CreateHttpClient()
    {
        httpClientBuilder ??= handler => new HttpClient(handler);
        HttpClient newClient = httpClientBuilder(new HttpClientHandler());
        newClient.BaseAddress = settings.Value.BaseUri;

        return newClient;
    }

    public async Task<PictureEmbeddingModel> GeneratePictureEmbeddingAsync(string mimeType, string pictureBase64)
        => await GeneratePictureEmbeddingAsync(mimeType, new MemoryStream(Convert.FromBase64String(pictureBase64)));

    public async Task<PictureEmbeddingModel> GeneratePictureEmbeddingAsync(string mimeType, Stream pictureDataStream)
    {
        const string uri = "api/v1/embedding/create";

        HttpResponseMessage response = await HttpClient.PostAsync(
            new Uri(uri, UriKind.Relative),
            new MultipartFormDataContent
            {
                { new StreamContent(pictureDataStream), "file", "file" },
                { new StringContent(mimeType), "mimeType" }
            });

        response.EnsureSuccessStatusCode();
        PictureEmbeddingModel? result = await response.Content.ReadFromJsonAsync<PictureEmbeddingModel>();

        return result ?? throw new InvalidOperationException("Failed to generate picture embedding.");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}