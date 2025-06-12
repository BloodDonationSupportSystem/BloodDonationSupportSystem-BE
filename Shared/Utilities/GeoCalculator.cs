using System;

namespace Shared.Utilities
{
    public static class GeoCalculator
    {
        // Earth radius in kilometers
        private const double EarthRadiusKm = 6371.0;

        /// <summary>
        /// Calculates the distance between two points on Earth using the Haversine formula
        /// </summary>
        /// <param name="lat1">Latitude of first point in decimal degrees</param>
        /// <param name="lon1">Longitude of first point in decimal degrees</param>
        /// <param name="lat2">Latitude of second point in decimal degrees</param>
        /// <param name="lon2">Longitude of second point in decimal degrees</param>
        /// <returns>Distance in kilometers</returns>
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Convert decimal degrees to radians
            var latRad1 = ToRadians(lat1);
            var lonRad1 = ToRadians(lon1);
            var latRad2 = ToRadians(lat2);
            var lonRad2 = ToRadians(lon2);

            // Haversine formula
            var dLat = latRad2 - latRad1;
            var dLon = lonRad2 - lonRad1;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(latRad1) * Math.Cos(latRad2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadiusKm * c;

            return distance;
        }

        /// <summary>
        /// Converts decimal degrees to radians
        /// </summary>
        /// <param name="degrees">Decimal degrees</param>
        /// <returns>Radians</returns>
        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}