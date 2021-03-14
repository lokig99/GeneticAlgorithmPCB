using System;
using System.Collections.Generic;
using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA.Utilities
{
    public class SolutionLogger : IGaCallback
    {
        public void Callback(Solution genBestSolution, double bestFitness, double worstFitness, int generationNumber,
            ICollection<double> generationIndividualsFitness)
        {
            Console.WriteLine($"Generation: {generationNumber}, best solution fitness: {bestFitness}");
        }
    }
}