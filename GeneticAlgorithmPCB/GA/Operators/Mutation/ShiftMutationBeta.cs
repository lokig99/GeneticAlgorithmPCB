using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class ShiftMutationBeta : IMutationOperator
    {
        public Random RandomGenerator { get; set; }
        private readonly ShiftMutationAlpha _mutationAlpha;
        public int MaxShift { get; set; } = 1;

        public ShiftMutationBeta(Random random, int maxShift)
        {
            MaxShift = maxShift;
            RandomGenerator = random;
            _mutationAlpha = new ShiftMutationAlpha
            {
                RandomGenerator = random,
                MaxShift = MaxShift
            };
        }

        public void Mutation(Solution solution, ISolutionInitializer initializer)
        {
            // select random segment
            var path = solution.Paths[RandomGenerator.Next(solution.Paths.Length)];

            var index = RandomGenerator.Next(path.Segments.Count);
            var segment = path.Segments[index];

            // random draw new segment length
            var len = RandomGenerator.Next(1, segment.Length);

            // calculate offset
            var offset = RandomGenerator.Next(0, (segment.Length - len) + 1);

            // split selected segment
            var (sx, sy) = segment.StartPoint;
            var (dx, dy) = segment.Direction.CoefficientTuple();
            var start = new Point(sx + dx * offset, sy + dy * offset);
            var end = new Point(start.X + len * dx, start.Y + len * dy);
            var pointsQuery = new[] { segment.StartPoint, segment.EndPoint, start, end }.Distinct();
            Point[] points;

            if (segment.Direction.GeneralDirection() == Direction.Vertical)
            {
                points = segment.Direction == Direction.Up
                    ? pointsQuery.OrderByDescending(p => p.Y).ToArray()
                    : pointsQuery.OrderBy(p => p.Y).ToArray();
            }
            else
            {
                points = segment.Direction == Direction.Left
                    ? pointsQuery.OrderByDescending(p => p.X).ToArray()
                    : pointsQuery.OrderBy(p => p.X).ToArray();
            }

            // create new tmp solution with start and end in selected segment
            var splitSegments = new List<Segment>();
            Segment modifiedSegment = null;
            var modifiedIndex = 0;
            for (var i = 0; i < points.Length - 1; i++)
            {
                var tmp = new Segment(points[i], segment.Direction,
                    (int)Point.Distance(points[i], points[i + 1]));

                if (tmp.StartPoint == start)
                {
                    modifiedSegment = tmp;
                    modifiedIndex = i;
                }

                splitSegments.Add(tmp);
            }

            // apply shiftMutationAlpha to tmp solution
            var tmpPath = new Path(start, end);
            tmpPath.Segments.Add(modifiedSegment?.Clone());
            var tmpSol = new Solution(solution.Problem, new Path[] { tmpPath });
            _mutationAlpha.Mutation(tmpSol, initializer);

            // append path of tmp solution
            splitSegments.InsertRange(modifiedIndex, tmpPath.Segments);
            splitSegments.RemoveAt(modifiedIndex + tmpPath.Segments.Count);

            path.Segments.InsertRange(index, splitSegments);
            path.Segments.RemoveAt(index + splitSegments.Count);

            // fix overlapping neighbors

            // previous
            if (index - 1 > 0)
            {
                var prev = path.Segments[index - 1];
                if (prev.Direction.OppositeDirection() == path.Segments[index].Direction)
                {
                    var tmpLen = prev.Length - path.Segments[index].Length;

                    switch (tmpLen)
                    {
                        case > 0:
                            prev.Length = tmpLen;
                            path.Segments.RemoveAt(index);
                            --index;
                            break;
                        case 0:
                            path.Segments.RemoveAt(index - 1);
                            path.Segments.RemoveAt(index - 1);
                            index -= 2;
                            break;
                        default:
                            prev.Length = -tmpLen;
                            prev.Direction = prev.Direction.OppositeDirection();
                            path.Segments.RemoveAt(index);
                            --index;
                            break;
                    }
                }
            }

            // next
            if (index + splitSegments.Count < path.Segments.Count)
            {
                var testedIndex = index + splitSegments.Count - 1;
                var next = path.Segments[index + splitSegments.Count];
                if (next.Direction.OppositeDirection() == path.Segments[testedIndex].Direction)
                {
                    var tmpLen = next.Length - path.Segments[testedIndex].Length;

                    switch (tmpLen)
                    {
                        case > 0:
                            next.Length = tmpLen;
                            next.StartPoint = path.Segments[testedIndex].StartPoint;
                            path.Segments.RemoveAt(testedIndex);
                            break;
                        case 0:
                            path.Segments.RemoveAt(testedIndex);
                            path.Segments.RemoveAt(testedIndex);
                            break;
                        default:
                            next.Length = -tmpLen;
                            next.Direction = next.Direction.OppositeDirection();
                            next.StartPoint = path.Segments[testedIndex].StartPoint;
                            path.Segments.RemoveAt(testedIndex);
                            break;
                    }
                }
            }


            // repair the path
            ShiftMutationAlpha.RepairPath(path);
        }
    }
}