using System;
using System.Collections.Generic;
using  SecureTransparentDataExchange.DTOs;
 using SecureTransparentDataExchange.Services.Realtime;

namespace SecureTransparentDataExchange.Services
{
    public class RouteService
    {
        private readonly RouteOptimizer _routeOptimizer;

        public RouteService()
        {
            _routeOptimizer = new RouteOptimizer();
        }

        public RouteModel GetOptimizedRoute(int[,] distances)
        {
            var optimizedRoute = _routeOptimizer.OptimizeRoutes(distances);

            var totalDistance = CalculateTotalDistance(optimizedRoute, distances);

            return new RouteModel
            {
                Route = optimizedRoute,
                TotalDistance = totalDistance
            };
        }

        private double CalculateTotalDistance(List<int> route, int[,] distances)
        {
            double totalDistance = 0;

            for (int i = 0; i < route.Count - 1; i++)
            {
                totalDistance += distances[route[i], route[i + 1]];
            }

            return totalDistance;
        }
    }
}
