using System;
using System.Collections.Generic;
using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA.Utilities
{
    public class SolutionLogger : IGaCallback
    {
        public void Callback(Solution genBestSolution, double allBestFitness, double genBestFitness,
            double genWorstFitness,
            int generationNumber, ICollection<(Solution solution, double fitness)> population)
        {
            Console.WriteLine(
                $"Generation: {generationNumber}, generation best fitness: {genBestFitness}, all time best fitness: {allBestFitness}");
        }
    }
}