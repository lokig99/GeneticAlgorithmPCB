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
            if (populationSize <= 1) throw new ArgumentOutOfRangeException(nameof(populationSize));
            if (generationLimit <= 0) throw new ArgumentOutOfRangeException(nameof(generationLimit));

            var population = InitializePopulation();
            var generation = 0;
            var best = BestSolution(population);
            var genBest = (best.solution, best.fitness);
            var genWorstFitness = WorstFitness(population);

            while (generation < generationLimit)
            {
                generation++;
                var newPopulation = new List<(Solution sol, double fitness)>(populationSize);
                var currentWorstFitness = double.MinValue;
                (Solution solution, double fitness)? currentBest = null;

                do
                {
                    var (parent1, _) = _selection.Selection(population, genBest, genWorstFitness);
                    var child = _crossover.Crossover(parent1, parent1);

                    _mutation.Mutation(child, _initializer);

                    var fitness = _fitness.Evaluate(child);

                    newPopulation.Add((child, fitness));

                    if (fitness > currentWorstFitness)
                        currentWorstFitness = fitness;

                    currentBest ??= (child, fitness);
                    if (fitness < currentBest.Value.fitness)
                        currentBest = (child, fitness);
                } while (newPopulation.Count != populationSize);

                var (sol, fit) = currentBest.Value;
                if (currentBest.Value.fitness < best.fitness)
                    best = (sol.Clone(), fit, generation);

                population = newPopulation;
                genWorstFitness = currentWorstFitness;
                genBest = ((Solution solution, double fitness))currentBest;

                foreach (var c in _callbacks)
                {
                    c.Callback(sol, best.fitness, fit, genWorstFitness,
                        generation, population);
                }
            }

            return best;

            List<(Solution sol, double fitness)> InitializePopulation()
            {
                return Enumerable.Range(0, populationSize)
                    .Select(_ => new Solution(Problem, _initializer))
                    .Select(s => (s, _fitness.Evaluate(s)))
                    .ToList();
            }

            (Solution solution, double fitness, int generation) BestSolution(
                IReadOnlyCollection<(Solution sol, double fitness)> pop)
            {
                var (sol, fitness) = pop.First(s =>
                    Math.Abs(s.fitness - pop.Min(pair => pair.fitness)) < 0.0001);

                return (sol.Clone(), fitness, generation);
            }

            static double WorstFitness(IEnumerable<(Solution sol, double fitness)> pop)
            {
                return pop.Max(p => p.fitness);
            }
        }
    }
}