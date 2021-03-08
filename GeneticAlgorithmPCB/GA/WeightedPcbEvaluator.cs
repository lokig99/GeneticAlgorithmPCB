using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA
{
    public class WeightedPcbEvaluator : IFitnessEvaluator
    {
        public double TotalLengthWeight { get; set; } = 1.0;
        public double SegmentCountWeight { get; set; } = 1.0;
        public double IntersectionWeight { get; set; } = 1.0;
        public double SegmentsOutsideBoardWeight { get; set; } = 1.0;
        public double FragmentsOutsideBoardWeight { get; set; } = 1.0;

        public double Evaluate(Solution solution, int boardWidth, int boardHeight)
        {
            var (sobCount, fobLength) = solution.SegmentsOutsideBoardStats(boardWidth, boardHeight);
            return solution.Intersections * IntersectionWeight + solution.TotalLength * TotalLengthWeight +
                   solution.TotalSegmentCount * SegmentCountWeight + sobCount * SegmentsOutsideBoardWeight +
                   fobLength * FragmentsOutsideBoardWeight;
        }
    }
}