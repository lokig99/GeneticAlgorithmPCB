using GeneticAlgorithmPCB.GA;
using GeneticAlgorithmPCB.GA.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            Console.Write("Enter path to problem file: ");
            var path = Console.ReadLine();
            var problem = PcbProblem.LoadProblemFromFile(path);
            var serializer = new GaSerializer(problem);

            var solver = new PcbGeneticSolver(problem, new WeightedEvaluator
            {
                FragmentsOutsideBoardWeight = 500.0,
                IntersectionWeight = 10000.0,
                SegmentCountWeight = 10.0,
                SegmentsOutsideBoardWeight = 5000.0,
                TotalLengthWeight = 1000.0
            }, new RandomSolutionInitializer(),
                new DummySelectionOperator(),
                new DummyCrossoverOperator(),
                new DummyMutationOperator(),
                new IGaCallback[] { new SolutionLogger(), serializer });

            var (_, fitness, gen) = solver.Solve(1, 1000);

            serializer.WaitOngoingSaves();

            Console.WriteLine($"Best result = {fitness} (gen = {gen})");

            GenerateSolutionVisualization();

            Console.WriteLine("Press enter to exit...");
            Console.ReadKey();


            void GenerateSolutionVisualization()
            {
                var pythonLauncherInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    UseShellExecute = false,
                    Arguments = $"generator.py {serializer.FilePaths.First()} result.avi",
                };

                Console.WriteLine("Generating video...");
                var process = Process.Start(pythonLauncherInfo);
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