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

        public Random RandomGenerator { get; set; } = new Random();

        public Solution Crossover(Solution sol1, Solution sol2)
        {
            var child = new Solution(sol1.Problem);

            for (var i = 0; i < child.Paths.Length; i++)
            {
                child.Paths[i] = RandomGenerator.NextDouble() < FirstParentProbability
                    ? sol1.Paths[i].Clone()
                    : sol2.Paths[i].Clone();
            }

            return child;
        }
    }
}