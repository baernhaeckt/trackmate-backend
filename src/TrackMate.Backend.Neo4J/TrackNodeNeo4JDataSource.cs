using Microsoft.Extensions.Options;
using Neo4j.Driver;
using Trackmate.Backend.TrackNodes;

namespace TrackMate.Backend.Neo4J;

public class TrackNodeNeo4JDataSource(IOptions<TrackNodeNeo4JDataSourceSettings> settings) : ITrackNodeDataSource
{
    private IDriver CreateDriver()
        => CreateDriver(settings.Value);

    private static IDriver CreateDriver(TrackNodeNeo4JDataSourceSettings settings)
        => GraphDatabase.Driver(settings.HostUri, AuthTokens.Basic(settings.Username, settings.Password));
}

