namespace TrackMate.Backend.Tests.Setup;

[CollectionDefinition(ApplicationCollection.Name)]
public class ApplicationCollection : ICollectionFixture<ApplicationFixture>
{
    public const string Name = "Application";
}