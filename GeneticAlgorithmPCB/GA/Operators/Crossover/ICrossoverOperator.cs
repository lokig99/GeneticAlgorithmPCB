namespace GeneticAlgorithmPCB.GA.Operators.Crossover
{
    public interface ICrossoverOperator
    {
        Solution Crossover(Solution sol1, Solution sol2, int? seed = null);
    }
}