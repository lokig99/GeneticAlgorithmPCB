using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithmPCB.GA.Interfaces;
using GeneticAlgorithmPCB.GA.Operators.Crossover;
using GeneticAlgorithmPCB.GA.Operators.Fitness;
using GeneticAlgorithmPCB.GA.Operators.Initialization;
using GeneticAlgorithmPCB.GA.Operators.Mutation;
using GeneticAlgorithmPCB.GA.Operators.Selection;

namespace GeneticAlgorithmPCB.GA
{
    public class PcbGeneticSolver
    {
        public PcbProblem Problem { get; set; }
        private readonly IFitnessEvaluator _fitness;
        private readonly ISolutionInitializer _initializer;
        private readonly IMutationOperator _mutation;
        private readonly ISelectionOperator _selection;
        private readonly ICrossoverOperator _crossover;
        private readonly IGaCallback[] _callbacks;

        public PcbGeneticSolver(PcbProblem problem, IFitnessEvaluator fitness, ISolutionInitializer initializer,
            ISelectionOperator selection, ICrossoverOperator crossover, IMutationOperator mutation,
            IGaCallback[] callbacks = null)
        {
            Problem = problem;
            _fitness = fitness;
            _initializer = initializer;
            _selection = selection;
            _crossover = crossover;
            _mutation = mutation;
            _callbacks = callbacks;
        }

        public (Solution best, double fitness, int generation) Solve(int populationSize, int generationLimit)
        {
            if (populationSize <= 0) throw new ArgumentOutOfRangeException(nameof(populationSize));
            if (generationLimit <= 0) throw new ArgumentOutOfRangeException(nameof(generationLimit));

            var population = InitializePopulation();
            var generation = 0;
            var best = BestSolution(population);

            while (generation < generationLimit)
            {
                generation++;
                (Solution Solution, double fitness)? generationBest = null;
                var newPopulation = new List<Solution>(populationSize);
                var generationIndividualFitness = new LinkedList<double>();
                var worstFitness = double.MinValue;

                do
                {
                    var (parent1, _) = _selection.Selection(population);
                    var child = _crossover.Crossover(parent1, parent1);

                    _mutation.Mutation(child);

                    var fitness = _fitness.Evaluate(child);
                    generationIndividualFitness.AddLast(fitness);
                    if (fitness < best.fitness)
                        best = (child, fitness, generation);

                    newPopulation.Add(child);

                    if (fitness > worstFitness)
                        worstFitness = fitness;

                    generationBest ??= (child, fitness);
                    if (fitness < generationBest.Value.fitness)
                        generationBest = (child, fitness);
                } while (newPopulation.Count != populationSize);

                population = newPopulation;

                var (sol, fit) = generationBest.Value;
                foreach (var c in _callbacks)
                {
                    c.Callback(sol, fit, worstFitness, generation, generationIndividualFitness);
                }
            }

            return best;

            List<Solution> InitializePopulation()
            {
                return Enumerable.Range(0, populationSize)
                    .Select(_ => new Solution(Problem, _initializer)).ToList();
            }

            (Solution solution, double fitness, int generation) BestSolution(IEnumerable<Solution> pop)
            {
                var solutionsEvaluated = pop.Select(s => new
                { s, fitness = _fitness.Evaluate(s) }).ToArray();

                var tmpBest = solutionsEvaluated.First(s =>
                    Math.Abs(s.fitness - solutionsEvaluated.Min(pair => pair.fitness)) < 0.0001);

                return (tmpBest.s, tmpBest.fitness, generation);
            }
        }
    }
}