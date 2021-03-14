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
        private const string FileNameTemplate = "ga_history_";
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

        private void SerializeChunk(IEnumerable historyCopy)
        {
            string jsonData;

            if (_chunkCount == 0)
            {
                var board = new[] { _problem.BoardWidth, _problem.BoardHeight };
                var points = _problem.PointPairs
                    .SelectMany(pair => new[] { pair.startPoint, pair.endPoint })
                    .Select(p => new[] { p.X, p.Y }).ToArray();
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

        public void Callback(Solution genBestSolution, double bestFitness, double worstFitness,
            int generationNumber,
            ICollection<double> generationIndividualsFitness)
        {
            _history.AddLast(CreateSerializedSolution(genBestSolution, generationNumber, bestFitness,
                generationIndividualsFitness, worstFitness));

            if (_history.Count < ChunkSize) return;

            var historyCopy = new object[_history.Count];
            _history.CopyTo(historyCopy, 0);
            _tasks.AddLast(Task.Run(() => SerializeChunk(historyCopy)));
            _history.Clear();
        }

        private static object CreateSerializedSolution(Solution genBestSolution, int generationNumber,
            double bestFitness,
            IEnumerable<double> generationIndividualsFitness, double worstFitness)
        {
            var paths = genBestSolution.Paths
                .Select(p => p.Segments.Select(s => new[] { s.StartPoint.X, s.StartPoint.Y }).ToList()).ToArray();
            for (var i = 0; i < paths.Length; i++)
            {
                var (x, y) = genBestSolution.Problem.PointPairs[i].endPoint;
                paths[i].Add(new[] { x, y });
            }

            return new
            {
                gen = generationNumber,
                fit = bestFitness,
                paths,
                gAvg = generationIndividualsFitness.Average(),
                wtF = worstFitness
            };
        }
    }
}