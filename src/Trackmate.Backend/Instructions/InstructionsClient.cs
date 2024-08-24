using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Trackmate.Backend.Instructions;

public class InstructionsClient(
    IOptions<InstructionsClientSettings> settings,
    Func<HttpClientHandler, HttpClient>? httpClientBuilder = null)
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

    public async Task<Stream> CreateInstructionAudioAsync(InstructionRequestModel instructionModel)
    {
        const string uri = "api/v1/instructions/generate_audio";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            new Uri(uri, UriKind.Relative),
            instructionModel);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<string> CreateInstructionTextAsync(InstructionRequestModel instructionModel)
    {
        const string uri = "api/v1/instructions/generate_text";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            new Uri(uri, UriKind.Relative),
            instructionModel);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}