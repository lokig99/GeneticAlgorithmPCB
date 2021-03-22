using System;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class ShiftMutationAlphaBeta : IMutationOperator
    {
        public Random RandomGenerator { get; set; }
        public double AlphaMutationChance { get; set; } = 0.5;
        public int MaxShift { get; set; } = 5;

        private readonly ShiftMutationBeta _mutationBeta;
        private readonly ShiftMutationAlpha _mutationAlpha;

        public ShiftMutationAlphaBeta(int maxShift)
        {
            MaxShift = maxShift;
            RandomGenerator = new Random();
            _mutationBeta = new ShiftMutationBeta(RandomGenerator, MaxShift);
            _mutationAlpha = new ShiftMutationAlpha { RandomGenerator = RandomGenerator, MaxShift = MaxShift };
        }

        public ShiftMutationAlphaBeta(int maxShift, Random randomGenerator)
        {
            MaxShift = maxShift;
            RandomGenerator = randomGenerator;
            _mutationBeta = new ShiftMutationBeta(RandomGenerator, MaxShift);
            _mutationAlpha = new ShiftMutationAlpha { RandomGenerator = RandomGenerator, MaxShift = MaxShift };
        }

        public void Mutation(Solution solution, ISolutionInitializer initializer)
        {
            if (AlphaMutationChance > RandomGenerator.NextDouble())
            {
                _mutationAlpha.Mutation(solution, initializer);
            }
            else
            {
                _mutationBeta.Mutation(solution, initializer);
            }
        }
    }
}