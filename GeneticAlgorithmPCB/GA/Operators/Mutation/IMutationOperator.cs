namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public interface IMutationOperator
    {
        Solution Mutation(Solution solution, double probability);
    }
}