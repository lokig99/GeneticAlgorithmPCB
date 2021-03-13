using GeneticAlgorithmPCB.GA;
using GeneticAlgorithmPCB.GA.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GeneticAlgorithmPCB
{
    public class Program
    {
        public const string OutputDir = "out";
        public const string TmpDir = "tmp";

        public static void Main()
        {
            var problem = PcbProblem.LoadProblemFromFile("zad1.txt");
            var solver = new PcbGeneticSolver(problem, new WeightedPcbEvaluator
            {
                FragmentsOutsideBoardWeight = 500.0,
                IntersectionWeight = 10000.0,
                SegmentCountWeight = 10.0,
                SegmentsOutsideBoardWeight = 5000.0,
                TotalLengthWeight = 1000.0
            }, new RandomSolutionInitializer(), new SolutionLogger());

            var ((_, fitness, gen), history) = solver.Solve(1, 1);

            Console.WriteLine($"Best result = {fitness} (gen = {gen})");
            Console.WriteLine("Generating solution output... (it may take a while)");
            GenerateSolutionVisualization(problem, history, 10000);
        }

        private static void GenerateSolutionVisualization(PcbProblem problem,
            ICollection<(Solution solution, double fitness, int generation)> solutions, int maxFramesLimit = 1000)
        {
            CreateDirectories();

            int step = (step = solutions.Count / maxFramesLimit) == 0 ? 1 : step;

            Console.WriteLine("Generating temporary data...");
            var solutionPath = $"{TmpDir}/solution.json";
            var imagePath = $"{OutputDir}/result.avi";

            CreateBoardVideo(problem,
                (from solPair in solutions
                        .Where((_, index) => index % step == 0 || index == solutions.Count - 1)
                 select (solPair.solution, solPair.fitness, solPair.generation)).ToArray(), solutionPath, imagePath);


            static void CreateBoardVideo(PcbProblem problem,
                ICollection<(Solution solution, double fitness, int gen)> solutions, string input,
                string output)
            {
                File.WriteAllText(input, SolutionSerializer.SerializeManyToJson(problem, solutions));

                var pythonLauncherInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    UseShellExecute = false,
                    Arguments = $"generator.py {input} {output}",
                };

                Console.WriteLine("Generating video...");
                var process = Process.Start(pythonLauncherInfo);
                process?.WaitForExit();
            }

            static void CreateDirectories()
            {
                if (!Directory.Exists(OutputDir))
                    Directory.CreateDirectory(OutputDir);

                if (!Directory.Exists(TmpDir))
                    Directory.CreateDirectory(TmpDir);
            }
        }
    }
}