using System;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class DummyMutationOperator : IMutationOperator
    {
        public void Mutation(Solution solution, ISolutionInitializer initializer)
        {
            const double probability = 0.5;
            var mutant = new Solution(solution.Problem, initializer);

            for (var i = 0; i < solution.Paths.Length; i++)
            {
                if (RandomGenerator.NextDouble() > probability)
                {
                    solution.Paths[i] = mutant.Paths[i];
                }
            }
        }

        public Random RandomGenerator { get; set; }
    }
}