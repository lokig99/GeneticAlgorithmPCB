using System;
using System.Collections.Generic;
using GeneticAlgorithmPCB.GA;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.Testing
{
    public class MutationTest
    {
        private Solution _solution;
        private PcbProblem _problem;

        public MutationTest()
        {
            _problem = new PcbProblem(16, 16,
                new List<(Point startPoint, Point endPoint)> { (new Point(0, 0), new Point(15, 15)) });
            _solution = new Solution(_problem, new RandomSolutionInitializer { RandomGenerator = new Random(1) });
        }


    }
}