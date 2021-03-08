namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface IMutationOperator
    {
        Solution Mutation(Solution solution, double probability);
    }
}