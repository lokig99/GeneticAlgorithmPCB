namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface ICrossoverOperator
    {
        Solution Crossover(Solution sol1, Solution sol2);
    }
}