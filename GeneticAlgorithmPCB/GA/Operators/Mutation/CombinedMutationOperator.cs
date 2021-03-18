using System;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class CombinedMutationOperator : IMutationOperator
    {
        public Random RandomGenerator { get; set; }
        public double RandomMutationChance { get; set; } = 0.5;
        public int MaxShift { get; set; } = 5;

        private readonly RandomMutationOperator _randomMutation;
        private readonly ShiftMutationOperator _shiftMutation;

        public CombinedMutationOperator()
        {
            RandomGenerator = new Random();
            _randomMutation = new RandomMutationOperator { RandomGenerator = RandomGenerator };
            _shiftMutation = new ShiftMutationOperator { RandomGenerator = RandomGenerator, MaxShift = MaxShift };
        }

        public CombinedMutationOperator(Random randomGenerator)
        {
            RandomGenerator = randomGenerator;
            _randomMutation = new RandomMutationOperator { RandomGenerator = RandomGenerator };
            _shiftMutation = new ShiftMutationOperator { RandomGenerator = RandomGenerator, MaxShift = MaxShift };
        }

        public void Mutation(Solution solution, ISolutionInitializer initializer)
        {
            if (RandomMutationChance > RandomGenerator.NextDouble())
            {
                _randomMutation.Mutation(solution, initializer);
            }
            else
            {
                _shiftMutation.Mutation(solution, initializer);
            }
        }
    }
}