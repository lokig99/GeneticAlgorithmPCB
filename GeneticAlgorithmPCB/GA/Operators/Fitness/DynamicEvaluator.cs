using System;
using System.Linq;

namespace GeneticAlgorithmPCB.GA.Operators.Fitness
{
    public class DynamicEvaluator : WeightedEvaluator
    {
        private readonly int[] _intersectionsBuffer;
        private int _bufferSize = 50;
        private int _bufferPointer;
        private readonly double _initialTotalLengthWeight;
        private readonly double _initialSegmentCountWeight;


        public int BufferSize
        {
            get => _bufferSize;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
                _bufferSize = value;
            }
        }

        public DynamicEvaluator()
        {
            _intersectionsBuffer = Enumerable.Repeat(int.MaxValue, BufferSize).ToArray();
            _initialSegmentCountWeight = SegmentCountWeight;
            _initialTotalLengthWeight = TotalLengthWeight;
        }

        private void AddBuffer(int intersections)
        {
            _bufferPointer %= BufferSize;
            _intersectionsBuffer[_bufferPointer++] = intersections;
        }

        public override double Evaluate(Solution solution)
        {
            AddBuffer(solution.Intersections);
            if (_intersectionsBuffer.Count(i => i == 0) > _bufferSize * 0.5)
            {
                TotalLengthWeight = _initialTotalLengthWeight;
                SegmentCountWeight = _initialSegmentCountWeight;
                IntersectionWeight *= 2;
            }
            else
            {
                TotalLengthWeight = 0;
                SegmentCountWeight = 0;
            }

            return base.Evaluate(solution);
        }
    }
}