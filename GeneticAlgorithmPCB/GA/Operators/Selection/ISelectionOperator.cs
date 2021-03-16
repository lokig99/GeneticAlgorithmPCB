using System.Collections.Generic;
using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA.Operators.Selection
{
    public interface ISelectionOperator : IRandom
    {
        (Solution parent1, Solution parent2) Selection(IList<(Solution sol, double fit)> population,
            (Solution solution, double fitness) populationBest, double worstFitness);
    }
}