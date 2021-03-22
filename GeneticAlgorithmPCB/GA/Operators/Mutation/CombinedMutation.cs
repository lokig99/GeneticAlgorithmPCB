using System;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class CombinedMutation : IMutationOperator
    {
        public Random RandomGenerator { get; set; }
        public double RandomMutationChance { get; set; } = 0.5;
        public int MaxShift { get; set; } = 5;

        private readonly RandomMutation _randomMutation;
        private readonly ShiftMutationAlpha _shiftMutation;

        public CombinedMutation()
        {
            RandomGenerator = new Random();
            _randomMutation = new RandomMutation { RandomGenerator = RandomGenerator };
            _shiftMutation = new ShiftMutationAlpha { RandomGenerator = RandomGenerator, MaxShift = MaxShift };
        }

        public CombinedMutation(Random randomGenerator)
        {
            RandomGenerator = randomGenerator;
            _randomMutation = new RandomMutation { RandomGenerator = RandomGenerator };
            _shiftMutation = new ShiftMutationAlpha { RandomGenerator = RandomGenerator, MaxShift = MaxShift };
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