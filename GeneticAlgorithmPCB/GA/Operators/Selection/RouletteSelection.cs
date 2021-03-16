using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithmPCB.GA.Operators.Selection
{
    public class RouletteSelection : ISelectionOperator
    {
        public double Bias { get; set; } = 1E3;
        public Random RandomGenerator { get; set; } = new Random();

        private T DrawItem<T>(IList<T> values, double[] prefix)
        {
            var drawn = RandomGenerator.NextDouble() * prefix.Last();
            var index = Array.BinarySearch(prefix, drawn);
            return values[index < 0 ? ~index : index];
        }

        public (Solution parent1, Solution parent2) Selection(IList<(Solution sol, double fit)> population,
            (Solution solution, double fitness) populationBest, double worstFitness)
        {
            var (_, bestFitness) = populationBest;
            var a = (0 - 1) / (bestFitness - worstFitness);
            var b = 0 - a * bestFitness;

            var frequency = GenerateFrequencyList();
            var prefix = CreatePrefix(frequency);

            var parent1 = DrawItem(population, prefix);
            var parent2 = DrawItem(population, prefix);
            return (parent1.sol, parent2.sol);

            List<double> GenerateFrequencyList()
            {
                return population.Select(p => Math.Pow(Bias, -NormalizeFitness(p.fit))).ToList();
            }

            double NormalizeFitness(double fitness)
            {
                return a * fitness + b;
            }

            static double[] CreatePrefix(IReadOnlyList<double> frequencyArray)
            {
                var prefix = new double[frequencyArray.Count];
                var tmp = 0d;
                for (var i = 0; i < prefix.Length; i++)
                {
                    prefix[i] = (frequencyArray[i] + tmp);
                    tmp += frequencyArray[i];
                }

                return prefix;
            }
        }
    }
}