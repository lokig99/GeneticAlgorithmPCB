using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA
{
    public struct PcbProblem
    {
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }
        public List<(Point startPoint, Point endPoint)> PointPairs { get; set; }

        public PcbProblem(int boardWidth, int boardHeight, List<(Point startPoint, Point endPoint)> points)
        {
            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
            PointPairs = points;
        }

        public readonly bool IsPointOutside(Point p)
        {
            var (x, y) = p;
            return x < 0 || y < 0 || x >= BoardWidth || y >= BoardHeight;
        }

        public static PcbProblem LoadProblemFromFile(string path)
        {
            var points = new List<(Point startPoint, Point endPoint)>();
            int[] dimensions;
            using (var stream = File.OpenText(path))
            {
                // get pcb board dimensions
                dimensions = stream.ReadLine()?.Split(';').Select(int.Parse).ToArray();
                if (dimensions == null || dimensions.Length != 2)
                    throw new FormatException();

                // get points to connect
                while (!stream.EndOfStream)
                {
                    var pairs = stream.ReadLine()?.Split(';')
                        .Select((n, index) => new { PairNum = index / 2, num = int.Parse(n) })
                        .GroupBy(pair => pair.PairNum)
                        .Select(grp => grp.Select(g => g.num).ToArray()).ToArray();

                    if (pairs == null || pairs.Length != 2 || pairs[0].Length != 2) throw new FormatException();
                    var p1 = new Point(pairs[0][0], pairs[0][1]);
                    var p2 = new Point(pairs[1][0], pairs[1][1]);
                    points.Add((p1, p2));
                }
            }

            return new PcbProblem(dimensions[0], dimensions[1], points);
        }
    }

    public class PcbGeneticSolver
    {
        public PcbProblem Problem { get; set; }
        private readonly IFitnessEvaluator _fitness;
        private readonly ISolutionInitializer _initializer;
        private readonly IGenerationCallback _callback;

        public PcbGeneticSolver(PcbProblem problem, IFitnessEvaluator fitness, ISolutionInitializer initializer,
            IGenerationCallback callback = null)
        {
            Problem = problem;
            _fitness = fitness;
            _initializer = initializer;
            _callback = callback;
        }

        public ((Solution best, double fitness, int generation),
            LinkedList<(Solution solution, double fitness, int generation)> history) Solve(
                int populationSize,
                int generationLimit)
        {
            var population = InitializePopulation(Problem, populationSize, _initializer);
            var generation = 1;
            var best = BestSolution(population);
            var history = new LinkedList<(Solution best, double fitness, int generation)>();
            history.AddLast(best);

            while (generation < generationLimit)
            {
                generation++;
                population = InitializePopulation(Problem, populationSize, _initializer);
                var tmpBest = BestSolution(population);
                history.AddLast(tmpBest);
                if (tmpBest.fitness < best.fitness)
                    best = tmpBest;
                _callback?.Callback(tmpBest.solution, tmpBest.fitness, generation, Problem, population);
            }

            return (best, history);

            static Solution[] InitializePopulation(PcbProblem problem, int size, ISolutionInitializer init)
            {
                var population = new Solution[size];
                for (var i = 0; i < size; i++)
                {
                    population[i] = new Solution(problem, init);
                }

                return population;
            }

            (Solution solution, double fitness, int generation) BestSolution(IEnumerable<Solution> pop)
            {
                var solEvaluated = pop.Select(s => new
                { s, fitness = _fitness.Evaluate(s, Problem.BoardWidth, Problem.BoardHeight) }).ToArray();

                var tmpBest = solEvaluated[0];
                for (var i = 1; i < solEvaluated.Length; i++)
                {
                    if (solEvaluated[i].fitness < tmpBest.fitness)
                        tmpBest = solEvaluated[i];
                }

                return (tmpBest.s, tmpBest.fitness, generation);
            }
        }
    }
}