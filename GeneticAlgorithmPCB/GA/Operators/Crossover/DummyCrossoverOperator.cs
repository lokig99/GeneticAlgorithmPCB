using System;

namespace GeneticAlgorithmPCB.GA.Operators.Crossover
{
    public class DummyCrossoverOperator : ICrossoverOperator
    {
        public Solution Crossover(Solution sol1, Solution sol2)
        {
            return sol1.Clone();
        }

        public Random RandomGenerator { get; set; }
    }
}