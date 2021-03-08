using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GeneticAlgorithmPCB.GA;

namespace GeneticAlgorithmPCB
{
    class Program
    {
        class SolutionSerializer
        {
            public int[] Board { get; set; }
            public int[][] Points { get; set; }
            public List<int[]>[] Paths { get; set; }

            public SolutionSerializer(PcbProblem problem, Solution solution)
            {
                Board = new[] { problem.BoardWidth, problem.BoardHeight };

                Points = problem.PointPairs
                    .SelectMany(pair => new Point[] { pair.startPoint, pair.endPoint })
                    .Select(p => new[] { p.X, p.Y }).ToArray();

                var tmp = solution.Paths
                    .Select(p => p.Segments.Select(s => new[] { s.StartPoint.X, s.StartPoint.Y }).ToList()).ToArray();
                for (var i = 0; i < tmp.Length; i++)
                {
                    var (x, y) = problem.PointPairs[i].endPoint;
                    tmp[i].Add(new[] { x, y });
                }

                Paths = tmp;
            }

            public string SerializeToJson()
            {
                return JsonSerializer.Serialize(this);
            }
        }


        static void Main(string[] args)
        {
            var problem = PcbProblem.LoadProblemFromFile("zad1.txt");

            var solver = new PcbGeneticSolver(problem, new WeightedPcbEvaluator(), new RandomPopInitializer());

            var ((solution, fitness), history) = solver.Solve(1, 10000);

            Console.WriteLine($"Best result = {fitness}");
            // var i = 0;
            // foreach (var oldBest in history)
            // {
            //     Console.WriteLine($"Generation: {i++}, best solution = {oldBest.fitness}");
            // }

            var serializer = new SolutionSerializer(problem, solution);
            Console.WriteLine(serializer.SerializeToJson());
        }
    }
}