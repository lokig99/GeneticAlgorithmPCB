using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithmPCB.GA.Operators.Selection
{
    public class TournamentSelection : ISelectionOperator
    {
        private double _populationPercentage = 0.1;

        public double PopulationPercentage
        {
            get => _populationPercentage;
            set
            {
                if (value <= 0 || value > 1) throw new ArgumentOutOfRangeException(nameof(value));
                _populationPercentage = value;
            }
        }

        public Random RandomGenerator { get; set; } = new Random();

        private void ShufflePopulation(IList<(Solution sol, double fit)> population)
        {
            for (var i = 0; i < population.Count; i++)
            {
                var swapIndex = RandomGenerator.Next(population.Count);
                (population[i], population[swapIndex]) = (population[swapIndex], population[i]);
            }
        }

        public (Solution parent1, Solution parent2) Selection(IList<(Solution sol, double fit)> population,
            (Solution solution, double fitness) populationBest, double worstFitness)
        {
            var count = (int)Math.Ceiling(population.Count * PopulationPercentage);

            var parent1 = RunTournament();
            var parent2 = RunTournament(parent1);

            return (parent1, parent2);

            Solution RunTournament(Solution excluded = null)
            {
                ShufflePopulation(population);
                (Solution sol, double fit)[] competitors;


                if (excluded is null)
                {
                    competitors = population.Take(count).ToArray();
                }
                else
                {
                    competitors = population
                        .Where(p => p.sol != excluded)
                        .Take(count).ToArray();
                }

                var winner = competitors.First();
                foreach (var competitor in competitors[1..])
                {
                    if (competitor.fit < winner.fit)
                        winner = competitor;
                }

                return winner.sol;
            }
        }
    }
}