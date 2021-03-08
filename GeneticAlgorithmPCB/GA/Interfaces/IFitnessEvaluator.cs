namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface IFitnessEvaluator
    {
        double Evaluate(Solution solution, int boardWidth, int boardHeight);
    }
}