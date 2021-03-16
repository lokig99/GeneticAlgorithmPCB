using GeneticAlgorithmPCB.GA;
using GeneticAlgorithmPCB.GA.Utilities;
using System;
using System.Diagnostics;
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
                    FragmentsOutsideBoardWeight = 500.0,
                    IntersectionWeight = 10000.0,
                    SegmentCountWeight = 10.0,
                    SegmentsOutsideBoardWeight = 1000.0,
                    TotalLengthWeight = 10.0
                },
                new RandomSolutionInitializer
                {
                    RandomGenerator = random
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
                new BaseMutationOperator()
                {
                    MaxShift = 16,
                    MutationChance = 0.2,
                    RandomGenerator = random
                },
                new IGaCallback[] { new SolutionLogger(), serializer });

            var (bestSolution, fitness, gen) = solver.Solve(250, 1500);

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
                File.WriteAllText(bestJsonPath, JsonSerializer.Serialize(bestSerialized));

                var pythonLauncherInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    UseShellExecute = false,
                    Arguments = $"generator.py image {bestJsonPath} {bestImgPath}",
                };

                Console.WriteLine("Generating best solution image...");
                var process = Process.Start(pythonLauncherInfo);
                process?.WaitForExit();

                //generate video of evolution
                var videoPath = $"{OutputDir}/result.avi";
                pythonLauncherInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    UseShellExecute = false,
                    Arguments = $"generator.py video {serializer.FilePaths.First()} {videoPath}",
                };

                Console.WriteLine("Generating video...");
                process = Process.Start(pythonLauncherInfo);
                process?.WaitForExit();
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