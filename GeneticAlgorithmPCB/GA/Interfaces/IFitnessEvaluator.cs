using System;
using System.Collections.Generic;
using System.Text;

namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface IFitnessEvaluator<in T>
    {
        double Evaluate(T solution);
    }
}
