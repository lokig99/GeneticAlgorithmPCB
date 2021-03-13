namespace GeneticAlgorithmPCB.GA.Interfaces
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}