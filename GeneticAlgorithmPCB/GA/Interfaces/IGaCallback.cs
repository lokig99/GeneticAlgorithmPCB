using System;
using System.Collections.Generic;
using System.Text;

namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface IGaCallback
    {
        void Callback(Solution genBestSolution, double bestFitness, double worstFitness, int generationNumber,
            ICollection<double> generationIndividualsFitness);
    }
}