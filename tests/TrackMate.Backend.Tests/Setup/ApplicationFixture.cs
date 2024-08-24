using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services.Extensions;

namespace TrackMate.Backend.Tests.Setup;

public sealed class ApplicationFixture
{
    private IContainerService neo4JContainer { get; set; }

    public ApplicationFixture()
    {
        const int Neo4JWebPort = 7474;
        const int Neo4JBoltPort = 7687;

        string startTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        neo4JContainer = new Builder()
            .UseContainer()
            .WithName($"neo4j{startTime}")
            .UseImage("neo4j")
            .ExposePort(Neo4JWebPort)
            .ExposePort(Neo4JBoltPort)
            .Wait("neo4j", (_, _) => 0)
            .Build()
            .Start();

        System.Net.IPEndPoint neo4JWebPort = neo4JContainer.ToHostExposedEndpoint($"{Neo4JWebPort}/tcp");
        System.Net.IPEndPoint neo4JBoltPort = neo4JContainer.ToHostExposedEndpoint($"{Neo4JBoltPort}/tcp");
    }
}
