using System.Numerics;

namespace Trackmate.Backend.Models;

public record GeoLocation(double Latitude, double Longitude, double Altitude)
{
    public Vector3 GetVector(GeoLocation target) 
        => CreateFrom(GeoLocationRadians.FromLocation(target)) - CreateFrom(GeoLocationRadians.FromLocation(this));

    public Vector3 AsCoordinates()
        => CreateFrom(GeoLocationRadians.FromLocation(this));

    private record GeoLocationRadians(double Latitude, double Longitude)
    {
        public static GeoLocationRadians FromLocation(GeoLocation location)
            => new GeoLocationRadians(DegreesToRadians(location.Latitude), DegreesToRadians(location.Longitude));

        private static double DegreesToRadians(double degrees)
            => degrees * Math.PI / 180.0;
    }

    private static Vector3 CreateFrom(GeoLocationRadians geoLocationRadians)
    {
        const double EarthRadius = 6371.0;

        return new Vector3(
            (float)(EarthRadius * Math.Cos(geoLocationRadians.Latitude) * Math.Cos(geoLocationRadians.Longitude)),
            (float)(EarthRadius * Math.Cos(geoLocationRadians.Latitude) * Math.Sin(geoLocationRadians.Longitude)),
            (float)(EarthRadius * Math.Sin(geoLocationRadians.Latitude)));
    }
}

