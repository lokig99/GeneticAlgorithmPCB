using System;
using System.Collections.Generic;
using System.Text;
using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA
{
    public class SolutionLogger : CallbackDecorator
    {
        public override void Callback(Solution solution, double fitness, int generationNumber, PcbProblem problem,
            Solution[] population)
        {
            base.Callback(solution, fitness, generationNumber, problem, population);
            Console.WriteLine($"Generation: {generationNumber}, best solution fitness: {fitness}");
        }

        public SolutionLogger(IGenerationCallback wrapped = null) : base(wrapped)
        {
        }
    }
}