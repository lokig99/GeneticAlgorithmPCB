using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GeneticAlgorithmPCB.GA;
using GeneticAlgorithmPCB.GA.Operators.Initialization;
using GeneticAlgorithmPCB.GA.Operators.Mutation;
using GeneticAlgorithmPCB.GA.Utilities;
using Xunit;

namespace GeneticAlgorithmPCB.Testing
{
    public class MutationTest
    {
        private Solution _solution;
        private PcbProblem _problem;
        private int _seed = 42;

        public MutationTest()
        {
            _problem = PcbProblem.LoadProblemFromFile("zad2.txt");
            _solution = new Solution(_problem, new RandomInitializer { RandomGenerator = new Random(1), InitialHeadToTargetProbability = 1, MaxLength = 20 });
        }

        [Fact]
        public void MutationTestingGround()
        {
            var mutation = new ShiftMutationBeta(new Random(_seed), 15);
            var init = new RandomInitializer { RandomGenerator = new Random(_seed) };
            var pathJson = "xd.json";
            var pathImg = "xd.png";

            for (var i = 0; i < 30; i++)
            {
                mutation.Mutation(_solution, init);

                var serialized = GaSerializer.CreateSerializedSolution(_solution, i, includeProblemInfo: true);
                File.WriteAllText(pathJson, JsonSerializer.Serialize(serialized));

                GaVisualizer.GenerateImage(pathJson, pathImg);
            }

            Assert.True(true);
        }


    }
}