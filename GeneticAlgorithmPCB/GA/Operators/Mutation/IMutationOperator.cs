namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public interface IMutationOperator
    {
        void Mutation(in Solution solution, in double probability = 0.5);
    }
}