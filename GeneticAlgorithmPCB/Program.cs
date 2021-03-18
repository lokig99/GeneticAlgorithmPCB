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

            var solver = new PcbGeneticSolver(problem,
                new WeightedEvaluator
                {
                    FragmentsOutsideBoardWeight = 20.0,
                    IntersectionWeight = 1000.0,
                    SegmentCountWeight = 1,
                    SegmentsOutsideBoardWeight = 10000,
                    TotalLengthWeight = 0
                },
                new RandomSolutionInitializer
                {
                    RandomGenerator = random,
                    MaxLength = 4
                },
                new RouletteSelection
                {
                    Bias = 10E4,
                    RandomGenerator = random
                },
                new UniformCrossoverOperator
                {
                    FirstParentProbability = 0.5,
                    RandomGenerator = random
                },
                new CombinedMutationOperator(random)
                {
                    MaxShift = 16,
                    RandomMutationChance = 0.5,
                },
                new IGaCallback[] { new SolutionLogger(), serializer })
            {
                MutationProbability = 0.5
            };

            var (bestSolution, fitness, gen) = solver.Solve(250, 1000);

            serializer.WaitOngoingSaves();

            Console.WriteLine($"Best result = {fitness} (gen = {gen})");
            Console.WriteLine($"Seed = {seed}");

            GenerateSolutionVisualization();

            Console.WriteLine("Press enter to exit...");
            Console.ReadKey();


            void GenerateSolutionVisualization()
            {
                // generate image of best solution
                var bestSerialized =
                    GaSerializer.CreateSerializedSolution(bestSolution, gen, fitness, includeProblemInfo: true);
                var bestJsonPath = $"{TmpDir}/best.json";
                var bestImgPath = $"{OutputDir}/best.png";
                var videoPath = $"{OutputDir}/result.avi";
                File.WriteAllText(bestJsonPath, JsonSerializer.Serialize(bestSerialized));

                Console.WriteLine("Generating best solution image...");
                GaVisualizer.GenerateImage(bestJsonPath, bestImgPath);

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