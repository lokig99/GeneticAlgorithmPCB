using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithmPCB.GA.Operators.Selection
{
    public class DummySelectionOperator : ISelectionOperator
    {
        public (Solution parent1, Solution parent2) Selection(IList<(Solution sol, double fit)> population,
            (Solution solution, double fitness) populationBest, double worstFitness)
        {
            return (population.First().sol, population.Last().sol);
        }

        public Random RandomGenerator { get; set; }
    }
}