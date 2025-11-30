using System;
using System.Xml;
using UnityEngine;

namespace X3D
{
    /// <summary>
    /// Handles X3D 4.0 Geographic Components: parsing and coordinate conversion.
    /// </summary>
    public static class X3DGeographicHandler
    {
        // WGS84 ellipsoid constants
        private const double a = 6378137.0; // Equatorial radius in meters
        private const double f = 1.0 / 298.257223563; // Flattening
        private const double b = a * (1 - f); // Polar radius

        /// <summary>
        /// Converts WGS84 (lat, lon, alt) to Unity world coordinates (ECEF, then to Unity axes).
        /// </summary>
        public static Vector3 GeoToUnity(double latitude, double longitude, double altitude)
        {
            // Convert degrees to radians
            double latRad = latitude * Math.PI / 180.0;
            double lonRad = longitude * Math.PI / 180.0;

            // ECEF conversion
            double e2 = 1 - (b * b) / (a * a);
            double N = a / Math.Sqrt(1 - e2 * Math.Sin(latRad) * Math.Sin(latRad));
            double x = (N + altitude) * Math.Cos(latRad) * Math.Cos(lonRad);
            double y = (N + altitude) * Math.Cos(latRad) * Math.Sin(lonRad);
            double z = ((b * b) / (a * a) * N + altitude) * Math.Sin(latRad);

            // Map ECEF to Unity axes (Unity: Y-up, ECEF: Z-up)
            return new Vector3((float)x, (float)z, (float)y);
        }

        /// <summary>
        /// Parses a GeoLocation node and returns the Unity position.
        /// </summary>
        public static Vector3 ParseGeoLocation(XmlNode geoLocationNode)
        {
            var geoCoords = geoLocationNode.Attributes["geoCoords"]?.Value;
            if (string.IsNullOrEmpty(geoCoords))
                throw new Exception("GeoLocation node missing geoCoords attribute.");
            var parts = geoCoords.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                throw new Exception("geoCoords must have at least latitude and longitude.");
            double lat = double.Parse(parts[0]);
            double lon = double.Parse(parts[1]);
            double alt = parts.Length > 2 ? double.Parse(parts[2]) : 0.0;
            return GeoToUnity(lat, lon, alt);
        }

        /// <summary>
        /// Parses a GeoViewpoint node and returns the Unity position and orientation.
        /// </summary>
        public static (Vector3 position, Quaternion rotation) ParseGeoViewpoint(XmlNode geoViewpointNode)
        {
            // TODO: Implement GeoViewpoint parsing and conversion
            return (Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Parses a GeoOrigin node and returns the Unity offset.
        /// </summary>
        public static Vector3 ParseGeoOrigin(XmlNode geoOriginNode)
        {
            // TODO: Implement GeoOrigin parsing and conversion
            return Vector3.zero;
        }

        /// <summary>
        /// Parses a GeoCoordinate node and returns an array of Unity positions.
        /// </summary>
        public static Vector3[] ParseGeoCoordinate(XmlNode geoCoordinateNode)
        {
            // TODO: Implement GeoCoordinate parsing and conversion
            return new Vector3[0];
        }

        /// <summary>
        /// Parses a GeoElevationGrid node and returns a grid of Unity positions.
        /// </summary>
        public static Vector3[,] ParseGeoElevationGrid(XmlNode geoElevationGridNode)
        {
            // TODO: Implement GeoElevationGrid parsing and conversion
            return new Vector3[0,0];
        }
    }
}
