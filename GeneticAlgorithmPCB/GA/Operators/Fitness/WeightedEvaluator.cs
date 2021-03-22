using System;
using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA.Operators.Fitness
{
    public class WeightedEvaluator : IFitnessEvaluator
    {
        public double TotalLengthWeight { get; set; } = 1.0;
        public double SegmentCountWeight { get; set; } = 1.0;
        public double IntersectionWeight { get; set; } = 1.0;
        public double SegmentsOutsideBoardWeight { get; set; } = 1.0;
        public double FragmentsOutsideBoardWeight { get; set; } = 1.0;

        public virtual double Evaluate(Solution solution)
        {
            var (sobCount, fobLength) = solution.SegmentsOutsideBoardStats;
            var fitness = solution.Intersections * IntersectionWeight
                          + solution.TotalLength * TotalLengthWeight
                          + solution.TotalSegmentCount * SegmentCountWeight
                          + sobCount * SegmentsOutsideBoardWeight
                          + fobLength * FragmentsOutsideBoardWeight;
            return fitness;
        }
    }
}