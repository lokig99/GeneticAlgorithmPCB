namespace GeneticAlgorithmPCB.GA.Operators.Crossover
{
    public class DummyCrossoverOperator : ICrossoverOperator
    {
        public Solution Crossover(Solution sol1, Solution sol2, int? seed = null)
        {
            return sol1.Clone();
        }
    }
}