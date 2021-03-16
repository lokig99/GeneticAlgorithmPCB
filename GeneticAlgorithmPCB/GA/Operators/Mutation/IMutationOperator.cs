using GeneticAlgorithmPCB.GA.Interfaces;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public interface IMutationOperator : IRandom
    {
        void Mutation(Solution solution, ISolutionInitializer initializer);
    }
}