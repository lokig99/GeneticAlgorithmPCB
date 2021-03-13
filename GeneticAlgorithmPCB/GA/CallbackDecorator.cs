using System;
using System.Collections.Generic;
using System.Text;
using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA
{
    public abstract class CallbackDecorator : IGenerationCallback
    {
        private readonly IGenerationCallback _wrapped;

        protected CallbackDecorator(IGenerationCallback wrapped = null)
        {
            _wrapped = wrapped;
        }

        public virtual void Callback(Solution solution, double fitness, int generationNumber, PcbProblem problem,
             Solution[] population)
        {
            _wrapped?.Callback(solution, fitness, generationNumber, problem, population);
        }
    }
}
