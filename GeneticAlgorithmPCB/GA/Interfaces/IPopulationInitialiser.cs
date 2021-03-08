namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface IPopulationInitializer
    {
        Solution[] Initialize(PcbProblem problem, int populationSize);
    }
}