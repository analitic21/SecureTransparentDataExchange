using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureTransparentDataExchange.Services
{
    public class RouteOptimizer
    {
        private readonly Random _random = new Random();

        public List<int> OptimizeRoutes(int[,] distances, int populationSize = 50, int generations = 100)
        {
            int numberOfPoints = distances.GetLength(0);

            List<List<int>> population = GenerateInitialPopulation(numberOfPoints, populationSize);

            for (int generation = 0; generation < generations; generation++)
            {
                population = population
                    .OrderBy(route => CalculateFitness(route, distances))
                    .ToList();

                population = EvolvePopulation(population, distances);
            }

            return population.First();
        }

        private List<List<int>> GenerateInitialPopulation(int numberOfPoints, int populationSize)
        {
            var population = new List<List<int>>();
            for (int i = 0; i < populationSize; i++)
            {
                var route = Enumerable.Range(0, numberOfPoints)
                    .OrderBy(x => _random.Next())
                    .ToList();
                population.Add(route);
            }
            return population;
        }

        private double CalculateFitness(List<int> route, int[,] distances)
        {
            double totalDistance = 0;

            for (int i = 0; i < route.Count - 1; i++)
            {
                totalDistance += distances[route[i], route[i + 1]];
            }

            return totalDistance;
        }

        private List<List<int>> EvolvePopulation(List<List<int>> population, int[,] distances)
        {
            var nextGeneration = new List<List<int>>();

            for (int i = 0; i < population.Count / 2; i++)
            {
                var parent1 = population[i];
                var parent2 = population[population.Count - i - 1];

                var child = Crossover(parent1, parent2);
                Mutate(child);

                nextGeneration.Add(child);
            }

            nextGeneration.AddRange(population.Take(2)); // elitism

            return nextGeneration;
        }

        private List<int> Crossover(List<int> parent1, List<int> parent2)
        {
            int cutPoint = _random.Next(parent1.Count);

            var child = new List<int>();
            child.AddRange(parent1.Take(cutPoint));
            child.AddRange(parent2.Except(child));

            return child;
        }

        private void Mutate(List<int> route)
        {
            if (_random.NextDouble() < 0.2)
            {
                int indexA = _random.Next(route.Count);
                int indexB = _random.Next(route.Count);

                var temp = route[indexA];
                route[indexA] = route[indexB];
                route[indexB] = temp;
            }
        }
    }
}
