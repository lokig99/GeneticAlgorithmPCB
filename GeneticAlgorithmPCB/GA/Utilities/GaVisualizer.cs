using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace GeneticAlgorithmPCB.GA.Utilities
{
    public static class GaVisualizer
    {
        public static int GenerateVideo(string inputPath, string outputPath)
        {
            var pythonLauncherInfo = new ProcessStartInfo
            {
                FileName = "python",
                UseShellExecute = false,
                Arguments = $"generator.py video {inputPath} {outputPath}",
            };

            var process = Process.Start(pythonLauncherInfo);
            process?.WaitForExit();
            return process?.ExitCode ?? 0;
        }

        public static void GenerateImage(string inputPath, string outputPath)
        {
            var pythonLauncherInfo = new ProcessStartInfo
            {
                FileName = "python",
                UseShellExecute = false,
                Arguments = $"generator.py image {inputPath} {outputPath}",
            };

            var process = Process.Start(pythonLauncherInfo);
            process?.WaitForExit();
        }
    }
}