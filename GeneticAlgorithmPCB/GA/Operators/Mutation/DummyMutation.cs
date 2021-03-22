using System;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class DummyMutation : IMutationOperator
    {
        public Random RandomGenerator { get; set; }
        public void Mutation(Solution solution, ISolutionInitializer initializer)
        {
            solution.ResetSolution(initializer);
        }
    }
}