using System.Collections.Generic;

namespace GeneticAlgorithmPCB.GA.Operators.Selection
{
    public interface ISelectionOperator
    {
        (Solution parent1, Solution parent2) Selection(ICollection<Solution> population);
    }
}