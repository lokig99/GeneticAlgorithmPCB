using System;
using System.Collections.Generic;
using System.Text;

namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface IGaCallback
    {
        void Callback(Solution genBestSolution, double allBestFitness, double genBestFitness, double genWorstFitness, int generationNumber,
            ICollection<(Solution solution, double fitness)> population);
    }
}