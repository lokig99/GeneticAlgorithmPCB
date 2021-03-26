using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GeneticAlgorithmPCB.GA.Interfaces;

// ReSharper disable InconsistentNaming

namespace GeneticAlgorithmPCB.GA.Utilities
{
    public class GaSerializer : IGaCallback
    {
        private const int ChunkSize = 10_000;
        public string FileNameTemplate { get; set; } = "ga_history_";
        private readonly LinkedList<object> _history;
        private int _chunkCount;
        private readonly LinkedList<Task> _tasks;
        private readonly PcbProblem _problem;
        public List<string> FilePaths { get; set; }

        public GaSerializer(PcbProblem problem)
        {
            _history = new LinkedList<object>();
            _tasks = new LinkedList<Task>();
            _problem = problem;
            FilePaths = new List<string>();
        }


        public void WaitOngoingSaves()
        {
            Task.WaitAll(_tasks.ToArray());

            if (_history.Count <= 0) return;
            SerializeChunk(_history);
            _history.Clear();
        }

        public static (int[] board, int[][] points) SerializeProblem(PcbProblem problem)
        {
            var board = new[] { problem.BoardWidth, problem.BoardHeight };
            var points = problem.PointPairs
                .SelectMany(pair => new[] { pair.startPoint, pair.endPoint })
                .Select(p => new[] { p.X, p.Y }).ToArray();
            return (board, points);
        }

        private void SerializeChunk(IEnumerable historyCopy)
        {
            string jsonData;

            if (_chunkCount == 0)
            {
                var (board, points) = SerializeProblem(_problem);
                jsonData = JsonSerializer.Serialize(new { board, points, data = historyCopy });
            }
            else
            {
                jsonData = JsonSerializer.Serialize(historyCopy);
            }

            var path = System.IO.Path.Join(Program.TmpDir, $"{FileNameTemplate}{++_chunkCount:D5}.json");
            FilePaths.Add(path);

            File.WriteAllText(path, jsonData);
        }

        public void WriteSerializedSolution(string path, Solution solution, int generation, double fitness)
        {
            // generate image of best solution
            var serializedObject =
                GaSerializer.CreateSerializedSolution(solution, generation, fitness, includeProblemInfo: true);

            File.WriteAllText(path, JsonSerializer.Serialize(serializedObject));
        }

        public static object CreateSerializedSolution(Solution solution, int? generationNumber,
            double bestFitness = 0, double avgFitness = 0, double worstFitness = 0, bool includeProblemInfo = false)
        {
            var paths = solution.Paths
                .Select(p => p.Segments.Select(s => new[] { s.StartPoint.X, s.StartPoint.Y }).ToList()).ToArray();
            for (var i = 0; i < paths.Length; i++)
            {
                var (x, y) = solution.Problem.PointPairs[i].endPoint;
                paths[i].Add(new[] { x, y });
            }

            if (!includeProblemInfo)
                return new
                {
                    gen = generationNumber,
                    fit = bestFitness,
                    paths,
                    gAvg = avgFitness,
                    wtF = worstFitness
                };

            var (board, points) = SerializeProblem(solution.Problem);
            return new
            {
                board = new[] { solution.Problem.BoardWidth, solution.Problem.BoardHeight },
                points,
                gen = generationNumber,
                fit = bestFitness,
                paths,
                gAvg = avgFitness,
                wtF = worstFitness
            };
        }

        public void Callback(Solution genBestSolution, double allBestFitness, double genBestFitness,
            double genWorstFitness,
            int generationNumber, ICollection<(Solution solution, double fitness)> population)
        {
            _history.AddLast(CreateSerializedSolution(genBestSolution, generationNumber, genBestFitness,
                population.Average(p => p.fitness), genWorstFitness));

            if (_history.Count < ChunkSize) return;

            var historyCopy = new object[_history.Count];
            _history.CopyTo(historyCopy, 0);
            _tasks.AddLast(Task.Run(() => SerializeChunk(historyCopy)));
            _history.Clear();
        }
    }
}