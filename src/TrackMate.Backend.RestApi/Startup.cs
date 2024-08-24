using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Instructions;
using Trackmate.Backend.Tracks;
using Trackmate.Backend;
using TrackMate.Backend.Neo4J;
using TrackMate.Backend.RestApi.Hubs;

namespace TrackMate.Backend.RestApi;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSignalR(opt =>
        {
            opt.EnableDetailedErrors = true;
            opt.MaximumReceiveMessageSize = 1024 * 1024 * 512; // 512kb
        });

        services
                .WithNeo4J(configuration)
                .Configure<PictureEmbeddingClientSettings>(configuration.GetRequiredSection(nameof(PictureEmbeddingClientSettings)))
                .Configure<InstructionsClientSettings>(configuration.GetRequiredSection(nameof(InstructionsClientSettings)))
                .AddSingleton<Func<HttpClientHandler, HttpClient>>(handler => new HttpClient(handler))
                .AddSingleton<PictureEmbeddingClient>()
                .AddSingleton<InstructionsClient>()
                .AddSingleton<ITrackDataSource, InMemoryTrackDataSource>()
                .AddSingleton<TrackNodeService>()
                .AddSingleton<TrackService>()
                .AddSingleton<TrackNodeHub>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors(x =>
        {
            x.AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(host => true)
                .WithExposedHeaders("Authorization")
                .AllowCredentials();
        });

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseEndpoints(endpoints => { endpoints.MapHub<TrackNodeHub>("/trackNodes"); });
    }
}