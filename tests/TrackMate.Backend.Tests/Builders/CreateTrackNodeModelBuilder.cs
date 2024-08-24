using Trackmate.Backend.Models;

namespace TrackMate.Backend.Tests.Builders;

public class CreateTrackNodeModelBuilder
{
    private GeoLocation _geoLocation = new GeoLocation(1.0, 2.0, 3.0);

    private TransformationVector _tranformationVector = new TransformationVector(4.0, 5.0, 6.0);

    private Orientation _orientation = new Orientation(7.0, 8.0);

    private Guid? _previousTrackNodeId;

    public static CreateTrackNodeModelBuilder Create() 
        => new CreateTrackNodeModelBuilder();

    public CreateTrackNodeModelBuilder WithGeoLocation(double latitude, double longitude, double altitude)
    {
        _geoLocation = new GeoLocation(latitude, longitude, altitude);
        return this;
    }

    public CreateTrackNodeModelBuilder WithTransformationVector(double x, double y, double z)
    {
        _tranformationVector = new TransformationVector(x, y, z);
        return this;
    }

    public CreateTrackNodeModelBuilder WithOrientation(double x, double y)
    {
        _orientation = new Orientation(x, y);
        return this;
    }

    public CreateTrackNodeModelBuilder WithPreviousTrackNodeId(Guid previousTrackNodeId)
    {
        _previousTrackNodeId = previousTrackNodeId;
        return this;
    }

    public CreateTrackNodeModel Build() 
        => new CreateTrackNodeModel(_geoLocation, _tranformationVector, _orientation);
}
