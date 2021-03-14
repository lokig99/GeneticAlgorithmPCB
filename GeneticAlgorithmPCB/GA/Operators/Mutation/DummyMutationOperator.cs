using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class DummyMutationOperator : IMutationOperator
    {
        public void Mutation(in Solution solution, in double probability = 0.5)
        {
            solution.ResetSolution(new RandomSolutionInitializer());
        }
    }
}