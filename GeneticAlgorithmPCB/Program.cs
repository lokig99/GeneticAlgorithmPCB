using GeneticAlgorithmPCB.GA;
using GeneticAlgorithmPCB.GA.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using GeneticAlgorithmPCB.GA.Interfaces;
using GeneticAlgorithmPCB.GA.Operators.Crossover;
using GeneticAlgorithmPCB.GA.Operators.Fitness;
using GeneticAlgorithmPCB.GA.Operators.Initialization;
using GeneticAlgorithmPCB.GA.Operators.Mutation;
using GeneticAlgorithmPCB.GA.Operators.Selection;

namespace GeneticAlgorithmPCB
{
    public class Program
    {
        public const string OutputDir = "out";
        public const string TmpDir = "tmp";

        internal static void Main()
        {
            CreateDirectories();
            Console.Write("Seed: ");
            var seedInput = Console.ReadLine();
            var seed = string.IsNullOrEmpty(seedInput) ? new Random().Next() : int.Parse(seedInput);

            Console.Write("Enter path to problem file: ");
            var path = Console.ReadLine();
            var problem = PcbProblem.LoadProblemFromFile(path);
            var serializer = new GaSerializer(problem);
            var random = new Random(seed);

            Console.Write("Enter population size (>1): ");
            var popSize = int.Parse(Console.ReadLine() ?? "100");
            Console.Write("Enter generation limit: ");
            var genLimit = int.Parse(Console.ReadLine() ?? "100");

            var solver = new PcbGeneticSolver(problem,
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
                    MaxLength = 8
                },
                new RouletteSelection()
                {
                    Bias = 10E9,
                    RandomGenerator = random
                },
                new UniformCrossoverOperator
                {
                    FirstParentProbability = 0.5,
                    RandomGenerator = random
                },
                new ShiftMutationAlphaBeta(16, random),
                new IGaCallback[] { new SolutionLogger(), serializer })
            {
                MutationProbability = 0.1
            };

            var (bestSolution, fitness, gen) = solver.Solve(popSize, genLimit);


            // var solver = new PcbGeneticSolver(problem,
            //     new WeightedEvaluator()
            //     {
            //         FragmentsOutsideBoardWeight = 20.0,
            //         IntersectionWeight = 1000.0,
            //         SegmentCountWeight = 10.0,
            //         SegmentsOutsideBoardWeight = 5000.0,
            //         TotalLengthWeight = 1
            //     },
            //     new RandomInitializer
            //     {
            //         RandomGenerator = random,
            //         MaxLength = 8
            //     },
            //     new DummySelectionOperator()
            //     {
            //         RandomGenerator = random
            //     },
            //     new DummyCrossoverOperator()
            //     {
            //         RandomGenerator = random
            //     },
            //     new DummyMutation
            //     {
            //         RandomGenerator = random
            //     },
            //     new IGaCallback[] { new SolutionLogger(), serializer })
            // {
            //     MutationProbability = 1.0
            // };
            //
            // var (bestSolution, fitness, gen) = solver.Solve(2, 500 * 500);


            serializer.WaitOngoingSaves();

            Console.WriteLine($"Best result = {fitness} (gen = {gen})");
            Console.WriteLine($"Seed = {seed}");

            GenerateBestSolutionImage();
            // GenerateSolutionVideo();

            Console.WriteLine("Press enter to exit...");
            Console.ReadKey();


            void GenerateBestSolutionImage()
            {
                var bestSerialized =
                    GaSerializer.CreateSerializedSolution(bestSolution, gen, fitness, includeProblemInfo: true);
                var bestJsonPath = $"{TmpDir}/best.json";
                var bestImgPath = $"{OutputDir}/best.png";
                File.WriteAllText(bestJsonPath, JsonSerializer.Serialize(bestSerialized));

                Console.WriteLine("Generating best solution image...");
                GaVisualizer.GenerateImage(bestJsonPath, bestImgPath);
            }


            void GenerateSolutionVideo()
            {
                var videoPath = $"{OutputDir}/result.avi";
                Console.WriteLine("Generating video...");
                GaVisualizer.GenerateVideo(serializer.FilePaths.First(), videoPath);
            }
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