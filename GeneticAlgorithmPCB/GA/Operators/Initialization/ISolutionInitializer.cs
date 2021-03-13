namespace GeneticAlgorithmPCB.GA.Operators.Initialization
{
    public interface ISolutionInitializer
    {
        void Initialize(in Solution solution, int? seed = null);
    }
}