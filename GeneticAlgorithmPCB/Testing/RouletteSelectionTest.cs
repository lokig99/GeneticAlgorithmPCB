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
    public class RouletteSelectionTest
    {
        private readonly List<(Solution sol, double fit)> _population;
        private const int Seed = 1;
        private readonly IFitnessEvaluator _fitness;
        private readonly Solution _best;
        private readonly double _bestFitness;
        private readonly double _worstFitness;

        public RouletteSelectionTest()
        {
            var points = new List<(Point startPoint, Point endPoint)>
            {
                (new Point(1, 3), new Point(5, 3)),
                (new Point(3, 1), new Point(3, 3))
            };

            var initializer = new RandomInitializer { RandomGenerator = new Random(Seed) };
            var problem = new PcbProblem(6, 6, points);

            var solutions = Enumerable.Range(0, 100)
                .Select(_ => new Solution(problem, initializer));

            _fitness = new WeightedEvaluator
            {
                FragmentsOutsideBoardWeight = 500.0,
                IntersectionWeight = 10000.0,
                SegmentCountWeight = 10.0,
                SegmentsOutsideBoardWeight = 5000.0,
                TotalLengthWeight = 1000.0
            };
            _population = solutions.Select(s => (s, _fitness.Evaluate(s))).ToList();
            var (best, fit) = _population.First(pair =>
                Math.Abs(pair.fit - _population.Min(p => p.fit)) < 0.00001);
            _bestFitness = fit;
            _best = best;

            var (_, worstFit) = _population.First(pair =>
                Math.Abs(pair.fit - _population.Max(p => p.fit)) < 0.00001);
            _worstFitness = worstFit;
        }

        [Fact]
        public void RouletteSelection_IsBestSolutionMostFrequentlyDrawn()
        {
            //prepare
            var results = new Dictionary<Solution, int>();
            const int repeats = 10000;

            var roulette = new RouletteSelection { RandomGenerator = new Random(Seed), Bias = 10E9 };

            for (var i = 0; i < repeats; i++)
            {
                var (d1, d2) = roulette.Selection(_population, (_best, _bestFitness), _worstFitness);

                results[d1] = results.GetValueOrDefault(d1, 0) + 1;
                results[d2] = results.GetValueOrDefault(d2, 0) + 1;
            }

            var mostFrequent = results
                .First(p => p.Value == results.Values.Max()).Key;

            Assert.Equal(_bestFitness, _fitness.Evaluate(mostFrequent));
        }
    }
}