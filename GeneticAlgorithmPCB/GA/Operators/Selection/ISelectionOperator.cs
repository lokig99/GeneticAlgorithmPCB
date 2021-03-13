using System.Collections.Generic;

namespace GeneticAlgorithmPCB.GA.Operators.Selection
{
    public interface ISelectionOperator
    {
        Solution Selection(ICollection<Solution> population);
    }
}