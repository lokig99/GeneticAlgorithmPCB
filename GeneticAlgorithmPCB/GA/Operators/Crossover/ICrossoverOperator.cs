using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA.Operators.Crossover
{
    public interface ICrossoverOperator : IRandom
    {
        Solution Crossover(Solution sol1, Solution sol2);
    }
}