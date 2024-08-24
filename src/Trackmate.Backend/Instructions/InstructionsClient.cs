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

    public async Task<InstructionAudioStream> CreateInstructionAsync(InstructionRequestModel instructionModel)
    {
        const string uri = "api/v1/instructions/create";

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            new Uri(uri, UriKind.Relative),
            instructionModel);

        response.EnsureSuccessStatusCode();

        return new InstructionAudioStream(await response.Content.ReadAsStreamAsync());
    }
}
