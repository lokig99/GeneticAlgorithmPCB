using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithmPCB.GA.Operators.Selection
{
    public class DummySelectionOperator : ISelectionOperator
    {
        public (Solution parent1, Solution parent2) Selection(ICollection<Solution> population)
        {
            return (population.First(), null);
        }
    }
}