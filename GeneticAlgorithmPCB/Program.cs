using GeneticAlgorithmPCB.GA;
using GeneticAlgorithmPCB.GA.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GeneticAlgorithmPCB.GA.Interfaces;
using GeneticAlgorithmPCB.GA.Operators.Crossover;
using GeneticAlgorithmPCB.GA.Operators.Fitness;
using GeneticAlgorithmPCB.GA.Operators.Initialization;
using GeneticAlgorithmPCB.GA.Operators.Mutation;
using GeneticAlgorithmPCB.GA.Operators.Selection;

namespace GeneticAlgorithmPCB
{
    internal class SolverConfig
    {
        public Random RandomGenerator { get; set; } = new Random();
        public PcbProblem Problem { get; set; }
        public double CrossoverProbability { get; set; } = 0.8;
        public double MutationProbability { get; set; } = 0.1;
        public int PopulationSize { get; set; } = 250;
        public int GenerationLimit { get; set; } = 1000;
        public int MaxSegmentLength { get; set; } = 8;
        public int MaxShift { get; set; } = 16;
    }


    public class Program
    {
        public const string OutputDir = "out";
        public const string TmpDir = "tmp";

        internal static ((Solution best, double fitness, int generation) result, long ElapsedMilliseconds) RunGa(
            SolverConfig config, int id = 1,
            bool showLogs = true)
        {
            var random = new Random(config.RandomGenerator.Next());
            var serializer = new GaSerializer(config.Problem) { FileNameTemplate = $"{id}_ga_history_" };
            var callbacks = new List<IGaCallback>(2) { serializer };

            if (showLogs) callbacks.Add(new SolutionLogger());

            var solver = new PcbGeneticSolver(config.Problem.Clone(),
                new WeightedEvaluator()
                {
                    FragmentsOutsideBoardWeight = 20.0,
                    IntersectionWeight = 1000.0,
                    SegmentCountWeight = 10.0,
                    SegmentsOutsideBoardWeight = 5000.0,
                    TotalLengthWeight = 1
                },
                new RandomInitializer
                {
                    RandomGenerator = random,
                    MaxLength = config.MaxSegmentLength
                },
                new RouletteSelection()
                {
                    Bias = 2,
                    RandomGenerator = random
                },
                new UniformCrossoverOperator
                {
                    FirstParentProbability = 0.5,
                    RandomGenerator = random
                },
                new ShiftMutationAlphaBeta(config.MaxShift, random),
                callbacks)
            {
                MutationProbability = config.MutationProbability,
                CrossoverProbability = config.CrossoverProbability
            };

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = solver.Solve(config.PopulationSize, config.GenerationLimit);
            watch.Stop();

            serializer.WaitOngoingSaves();
            return (result, watch.ElapsedMilliseconds);
        }
        internal static ((Solution best, double fitness, int generation) result, long ElapsedMilliseconds) RunRandom(
    SolverConfig config, int id = 1,
    bool showLogs = true)
        {
            var random = new Random(config.RandomGenerator.Next());
            var serializer = new GaSerializer(config.Problem) { FileNameTemplate = $"{id}_ga_history_" };
            var callbacks = new List<IGaCallback>(2) { serializer };

            if (showLogs) callbacks.Add(new SolutionLogger());

            var solver = new PcbGeneticSolver(config.Problem.Clone(),
                new WeightedEvaluator()
                {
                    FragmentsOutsideBoardWeight = 20.0,
                    IntersectionWeight = 1000.0,
                    SegmentCountWeight = 10.0,
                    SegmentsOutsideBoardWeight = 5000.0,
                    TotalLengthWeight = 1
                },
                new RandomInitializer
                {
                    RandomGenerator = random,
                    MaxLength = config.MaxSegmentLength
                },
                new DummySelectionOperator(),
                new DummyCrossoverOperator(),
                new DummyMutation(),
                callbacks)
            {
                MutationProbability = 1,
                CrossoverProbability = 0
            };

            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = solver.Solve(config.PopulationSize, config.GenerationLimit);
            watch.Stop();

            serializer.WaitOngoingSaves();
            return (result, watch.ElapsedMilliseconds);
        }


        internal static void Main()
        {
            CreateDirectories();

            Console.Write("Seed: ");
            var input = Console.ReadLine();
            var seed = string.IsNullOrEmpty(input) ? new Random().Next() : int.Parse(input);
            var random = new Random(seed);

            Console.Write("Enter path to problem file: ");
            var path = Console.ReadLine();
            var problem = PcbProblem.LoadProblemFromFile(path);

            Console.Write("Enter population size (>1, default: 250): ");
            input = Console.ReadLine();
            var popSize = string.IsNullOrEmpty(input) ? 250 : int.Parse(input);

            Console.Write("Enter generation limit (default: 1000): ");
            input = Console.ReadLine();
            var genLimit = string.IsNullOrEmpty(input) ? 1000 : int.Parse(input);

            Console.Write("Enter mutation probability ([0.0-1.0], default: 0.1): ");
            input = Console.ReadLine();
            var mutProb = string.IsNullOrEmpty(input) ? 0.1 : double.Parse(input);

            Console.Write("Enter crossover probability ([0.0-1.0], default: 0.8): ");
            input = Console.ReadLine();
            var crossProb = string.IsNullOrEmpty(input) ? 0.8 : double.Parse(input);

            Console.Write("Enter number of passes (default: 1): ");
            input = Console.ReadLine();
            var passes = string.IsNullOrEmpty(input) ? 1 : int.Parse(input);
            var results = new ((Solution bestSolution, double fitness, int gen) result, long time)[passes];
            var tasks = new Task[passes];

            var config = new SolverConfig()
            {
                CrossoverProbability = crossProb,
                GenerationLimit = genLimit,
                MaxSegmentLength = 8,
                MutationProbability = mutProb,
                PopulationSize = popSize,
                Problem = problem,
                RandomGenerator = random
            };

            for (var i = 0; i < results.Length; i++)
            {
                Console.WriteLine($"\n\n---ROUND {i + 1}---\n");
                var ii = i;
                var showLogs = i == 0;
                tasks[i] = Task.Run((() => results[ii] = RunGa(config, ii + 1, showLogs)));
            }

            Task.WaitAll(tasks);

            // results
            Console.WriteLine("\n\n------------RESULTS------------\n");
            var min = results.Min(rr => rr.result.fitness);
            var (((bestSolution, fitness, gen), time), id) = results
                .Select((tuple, i) => (tuple, i))
                .First(t => t.tuple.result.fitness <= min);
            var avg = results.Average(r => r.result.fitness);
            var std = Math.Sqrt(results.Average(r => r.result.fitness * r.result.fitness) - avg * avg);
            var avgTime = results.Average(r => r.time);
            Console.WriteLine($"Best result = {fitness}");
            Console.WriteLine($"Worst solution = {results.Max(r => r.result.fitness)}");
            Console.WriteLine($"Average = {avg:F2}");
            Console.WriteLine($"Standard Deviation = {std:F2}");
            Console.WriteLine($"Average execution time = {avgTime / 1000:F2} seconds");
            Console.WriteLine("\nAll results:");
            foreach (var res in results)
            {
                Console.WriteLine(res.result.fitness);
            }

            Console.WriteLine($"\nSeed = {seed}");

            GenerateBestSolutionImage($"{id}-best", bestSolution, gen, fitness);

            Console.WriteLine("Press enter to exit...");
            Console.ReadKey();


            // void GenerateSolutionVideo()
            // {
            //     var videoPath = $"{OutputDir}/result.avi";
            //     Console.WriteLine("Generating video...");
            //     GaVisualizer.GenerateVideo(serializer.FilePaths.First(), videoPath);
            // }
        }


        internal static void GenerateBestSolutionImage(string imageName, Solution solution, int generation,
            double fitness)
        {
            var bestSerialized =
                GaSerializer.CreateSerializedSolution(solution, generation, fitness, includeProblemInfo: true);
            var bestJsonPath = $"{TmpDir}/{imageName}.json";
            var bestImgPath = $"{OutputDir}/{imageName}.png";
            File.WriteAllText(bestJsonPath, JsonSerializer.Serialize(bestSerialized));

            Console.WriteLine("Generating best solution image...");
            GaVisualizer.GenerateImage(bestJsonPath, bestImgPath);
        }

        internal static void CreateDirectories()
        {
            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);

            if (!Directory.Exists(TmpDir))
                Directory.CreateDirectory(TmpDir);
        }
    }
}