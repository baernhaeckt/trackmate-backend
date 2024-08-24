using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trackmate.Backend.TrackNodes;
using TrackMate.Backend.Neo4J;
using TrackMate.Backend.RestApi;

namespace TrackMate.Backend.Tests.Setup;

public class TrackMateWebApplicationFactory(TrackNodeNeo4JDataSourceSettings neo4JSettings) : WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .ConfigureServices(services => {
                services.AddSingleton<ITrackNodeDataSource>(new TrackNodeNeo4JDataSource(Options.Create(neo4JSettings)));
        });

        base.ConfigureWebHost(builder);
    }

    protected override TestServer CreateServer(IWebHostBuilder builder)
    {
        return base.CreateServer(builder);
    }
}
