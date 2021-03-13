using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

// ReSharper disable InconsistentNaming

namespace GeneticAlgorithmPCB.GA.Utilities
{
    public static class SolutionSerializer
    {
        private static object CreateSerializedSolution(PcbProblem problem, Solution solution,
            int generation, double fitness)
        {
            var paths = solution.Paths
                .Select(p => p.Segments.Select(s => new[] { s.StartPoint.X, s.StartPoint.Y }).ToList()).ToArray();
            for (var i = 0; i < paths.Length; i++)
            {
                var (x, y) = problem.PointPairs[i].endPoint;
                paths[i].Add(new[] { x, y });
            }

            return new { generation, fitness, paths };
        }

        public static string SerializeToJson(PcbProblem problem, Solution solution, int generation, double fitness,
            bool prettyPrint = false)
        {
            var serSolution = CreateSerializedSolution(problem, solution, generation, fitness);
            return JsonSerializer.Serialize(serSolution,
                new JsonSerializerOptions { WriteIndented = prettyPrint });
        }

        public static string SerializeManyToJson(PcbProblem problem,
            ICollection<(Solution solution, double fitness, int gen)> solutions,
            bool prettyPrint = false)
        {
            var board = new[] { problem.BoardWidth, problem.BoardHeight };

            var points = problem.PointPairs
                .SelectMany(pair => new Point[] { pair.startPoint, pair.endPoint })
                .Select(p => new[] { p.X, p.Y }).ToArray();

            var data = solutions.Select((pair =>
                CreateSerializedSolution(problem, pair.solution, pair.gen, pair.fitness)));

            return JsonSerializer.Serialize(new { board, points, data },
                new JsonSerializerOptions() { WriteIndented = prettyPrint });
        }
    }
}