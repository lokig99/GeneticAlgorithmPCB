using System;
using System.Collections.Generic;
using System.Text;

namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface IGaCallback
    {
        void Callback(Solution solution, double fitness, int generationNumber, PcbProblem problem,
             Solution[] population);
    }
}