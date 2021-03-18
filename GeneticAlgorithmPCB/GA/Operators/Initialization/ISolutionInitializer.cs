using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA.Operators.Initialization
{
    public interface ISolutionInitializer : IRandom
    {
        void Initialize(in Solution solution);
        Path GeneratePath(PcbProblem problem, int pathIndex);
    }
}