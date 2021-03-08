using System.Collections.Generic;


namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface ISelectionOperator
    {
        Solution Selection(IEnumerable<Solution> population);
    }
}