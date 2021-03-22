using System;
using System.Collections.Generic;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class RandomMutation : IMutationOperator
    {
        public Random RandomGenerator { get; set; } = new Random();

        public void Mutation(Solution solution, ISolutionInitializer initializer)
        {
            var path = solution.Paths[RandomGenerator.Next(solution.Paths.Length)];
            var segments = path.Segments;
            var index = RandomGenerator.Next(segments.Count);

            // pick segment
            var startPoint = segments[index].StartPoint;
            var endPoint = segments[index].EndPoint;

            // remove original segment
            segments.RemoveAt(index);

            // create new random sub-path
            var h = solution.Problem.BoardHeight;
            var w = solution.Problem.BoardWidth;
            var tmpProblem = new PcbProblem(w, h,
                new List<(Point startPoint, Point endPoint)>()
                {
                    (startPoint, endPoint)
                });

            var newSegments = initializer.GeneratePath(tmpProblem, 0).Segments;

            // insert new sub-path in place of old one
            segments.InsertRange(index, newSegments);
            var lastIndex = index + newSegments.Count;

            // merge segments if in the same direction
            if (index - 1 >= 0 && segments[index - 1].Direction == segments[index].Direction)
            {
                segments[index - 1].Length += segments[index].Length;
                segments.RemoveAt(index);
                lastIndex--;
            }

            if (lastIndex + 1 >= segments.Count ||
                segments[lastIndex].Direction != segments[lastIndex + 1].Direction) return;

            segments[lastIndex].Length += segments[lastIndex + 1].Length;
            segments.RemoveAt(lastIndex + 1);
        }
    }
}