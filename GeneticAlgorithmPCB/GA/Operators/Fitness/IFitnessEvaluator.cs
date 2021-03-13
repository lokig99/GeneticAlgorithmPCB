namespace GeneticAlgorithmPCB.GA.Operators.Fitness
{
    public interface IFitnessEvaluator
    {
        double Evaluate(Solution solution, int boardWidth, int boardHeight);
    }
}