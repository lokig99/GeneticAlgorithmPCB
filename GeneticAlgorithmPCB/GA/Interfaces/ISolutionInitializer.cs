namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface ISolutionInitializer
    {
        void Initialize(in Solution solution, int? seed = null);
    }
}