using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services.Extensions;
using TrackMate.Backend.Neo4J;

namespace TrackMate.Backend.Tests.Setup;

public sealed class ApplicationFixture
{
    private IContainerService neo4JContainer { get; set; }

    public int Neo4JWebPort { get; private set; }

    public int Neo4JBoltPort { get; private set; }

    public TrackNodeNeo4JDataSourceSettings Neo4JDataSourceSettings
    {
        get
        {
            return new TrackNodeNeo4JDataSourceSettings
            {
                Uri = $"bolt://localhost:{Neo4JBoltPort}",
                Username = "neo4j",
                Password = "neo4j"
            };
        }
    }

    public ApplicationFixture()
    {
        const int Neo4JContainerWebPort = 7474;
        const int Neo4JContainerBoltPort = 7687;

        string startTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        neo4JContainer = new Builder()
            .UseContainer()
            .WithName($"neo4j{startTime}")
            .KeepRunning()
            .UseImage("neo4j:5.23-enterprise")
            .WithEnvironment("NEO4J_PLUGINS='[\"graph-data-science\"]'", "NEO4J_ACCEPT_LICENSE_AGREEMENT=yes", "NEO4J_AUTH=none")
            .ExposePort(Neo4JContainerWebPort)
            .ExposePort(Neo4JContainerBoltPort)
            .WaitForMessageInLog("Started")
            .Build()
            .Start();

        System.Net.IPEndPoint neo4JWebPort = neo4JContainer.ToHostExposedEndpoint($"{Neo4JContainerWebPort}/tcp");
        Neo4JWebPort = neo4JWebPort.Port;
        System.Net.IPEndPoint neo4JBoltPort = neo4JContainer.ToHostExposedEndpoint($"{Neo4JContainerBoltPort}/tcp");
        Neo4JBoltPort = neo4JBoltPort.Port;
    }
}
