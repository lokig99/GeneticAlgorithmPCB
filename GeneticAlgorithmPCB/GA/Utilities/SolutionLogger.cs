using System;
using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA.Utilities
{
    public class SolutionLogger : IGaCallback
    {
        public void Callback(Solution solution, double fitness, int generationNumber, PcbProblem problem,
            Solution[] population)
        {
            Console.WriteLine($"Generation: {generationNumber}, best solution fitness: {fitness}");
        }
    }
}