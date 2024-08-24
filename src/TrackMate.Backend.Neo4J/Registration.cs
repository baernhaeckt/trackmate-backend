using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trackmate.Backend.TrackNodes;

namespace TrackMate.Backend.Neo4J;

public static class Registration
{
    public static IServiceCollection WithNeo4J(this IServiceCollection services, IConfiguration configuration)
        => services
                .Configure<TrackNodeNeo4JDataSourceSettings>(configuration.GetRequiredSection(nameof(TrackNodeNeo4JDataSourceSettings)))
                .AddSingleton<ITrackNodeDataSource, TrackNodeNeo4JDataSource>();
}
