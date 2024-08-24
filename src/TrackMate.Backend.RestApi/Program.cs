using Trackmate.Backend;
using Trackmate.Backend.Embeddings;
using Trackmate.Backend.Instructions;
using Trackmate.Backend.TrackNodes;
using Trackmate.Backend.Tracks;
using TrackMate.Backend.Neo4J;
using TrackMate.Backend.RestApi.Hubs;

namespace TrackMate.Backend.RestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .WithNeo4J(builder.Configuration)
                .Configure<PictureEmbeddingClientSettings>(builder.Configuration.GetRequiredSection(nameof(PictureEmbeddingClientSettings)))
                .Configure<InstructionsClientSettings>(builder.Configuration.GetRequiredSection(nameof(InstructionsClientSettings)))
                .AddSingleton<Func<HttpClientHandler, HttpClient>>(handler => new HttpClient(handler))
                .AddSingleton<PictureEmbeddingClient>()
                .AddSingleton<InstructionsClient>()
                .AddSingleton<ITrackDataSource, InMemoryTrackDataSource>()
                .AddSingleton<TrackNodeService>()
                .AddSingleton<TrackNodeHub>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSignalR(opt =>
            {
                opt.EnableDetailedErrors = true;
                opt.MaximumReceiveMessageSize = 1024 * 1024 * 512; // 512kb
            });

            var app = builder.Build();

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
            app.MapControllers();

            app.UseEndpoints(endpoints => { endpoints.MapHub<TrackNodeHub>("/trackNodes"); });

            app.Run();
        }
    }
}
