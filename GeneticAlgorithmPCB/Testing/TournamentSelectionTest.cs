using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithmPCB.GA;
using GeneticAlgorithmPCB.GA.Operators.Fitness;
using GeneticAlgorithmPCB.GA.Operators.Initialization;
using GeneticAlgorithmPCB.GA.Operators.Selection;
using Xunit;

namespace GeneticAlgorithmPCB.Testing
{
    public class TournamentSelectionTest
    {
        private readonly List<(Solution sol, double fit)> _population;
        private readonly PcbProblem _problem;
        private const int Seed = 1;
        private readonly RandomSolutionInitializer _initializer;
        private readonly IFitnessEvaluator _fitness;

        public TournamentSelectionTest()
        {
            var points = new List<(Point startPoint, Point endPoint)>
            {
                (new Point(1, 3), new Point(5, 3)),
                (new Point(3, 1), new Point(3, 3))
            };

            _initializer = new RandomSolutionInitializer { RandomGenerator = new Random(Seed) };
            _problem = new PcbProblem(6, 6, points);

            var solutions = Enumerable.Range(0, 100)
                .Select(_ => new Solution(_problem, _initializer));

            _fitness = new WeightedEvaluator
            {
                FragmentsOutsideBoardWeight = 500.0,
                IntersectionWeight = 10000.0,
                SegmentCountWeight = 10.0,
                SegmentsOutsideBoardWeight = 5000.0,
                TotalLengthWeight = 1000.0
            };

            _population = solutions.Select(s => (s, _fitness.Evaluate(s))).ToList();
        }

        [Fact]
        public void TournamentSelection_WholePopulation_ReturnsTwoBestFitnessInPopulation()
        {
            var tournament = new TournamentSelection { PopulationPercentage = 1, RandomGenerator = new Random(Seed) };

            var (winner, winner2) = tournament.Selection(_population, (null, 0), 0);

            var (best1, _) = _population.First(pair => Math.Abs(pair.fit - _population.Min(p => p.fit)) < 0.00001);

            var popNoWinner1 = _population.Where(p => p.sol != best1).ToArray();
            var (best2, _) = popNoWinner1
                .First(pair => Math.Abs(pair.fit - popNoWinner1.Min(p => p.fit)) < 0.00001);

            Assert.True(winner == best1);
            Assert.True(winner2 == best2);
        }
    }
}