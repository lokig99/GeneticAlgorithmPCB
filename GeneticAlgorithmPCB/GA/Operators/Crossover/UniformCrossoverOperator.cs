using System;

namespace GeneticAlgorithmPCB.GA.Operators.Crossover
{
    public class UniformCrossoverOperator : ICrossoverOperator
    {
        private double _firstParentProbability = 0.5;
        public double FirstParentProbability
        {
            get => _firstParentProbability;
            set
            {
                if (value > 1 || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _firstParentProbability = value;
            }
        }

        public Solution Crossover(Solution sol1, Solution sol2, int? seed = null)
        {
            var child = new Solution(sol1.Problem);
            var rand = seed is null ? new Random() : new Random((int)seed);

            for (var i = 0; i < child.Paths.Length; i++)
            {
                child.Paths[i] = rand.NextDouble() < FirstParentProbability
                    ? sol1.Paths[i].Clone()
                    : sol2.Paths[i].Clone();
            }

            return child;
        }
    }
}