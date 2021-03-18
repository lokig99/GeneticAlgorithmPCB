using System;
using System.Linq;

namespace GeneticAlgorithmPCB.GA.Operators.Initialization
{
    public class RandomSolutionInitializer : ISolutionInitializer
    {
        public Random RandomGenerator { get; set; } = new Random();
        public int MaxLength { get; set; } = 5;
        public double InitialHeadToTargetProbability { get; set; } = 0.0;


        public void Initialize(in Solution solution)
        {
            solution.Paths = new Path[solution.Problem.PointPairs.Count];

            for (var i = 0; i < solution.Paths.Length; i++)
            {
                solution.Paths[i] = GeneratePath(solution.Problem, i);
            }
        }

        public Path GeneratePath(PcbProblem problem, int pathIndex)
        {
            var (startPoint, endPoint) = problem.PointPairs[pathIndex];
            var path = new Path(startPoint, endPoint);
            var headToEndProb = InitialHeadToTargetProbability;
            path.Segments.Add(RandomSegment(startPoint));

            while (!HasReachedTarget(path))
            {
                var prevSegment = path.Segments.Last();
                var segment = RandomSegment(prevSegment.EndPoint);

                if (segment.Direction.GeneralDirection() == prevSegment.Direction.GeneralDirection())
                {
                    if (segment.Direction == prevSegment.Direction)
                    {
                        prevSegment.Length += segment.Length;
                    }
                    else
                    {
                        var tmp = prevSegment.Length - segment.Length;
                        prevSegment.Length = tmp > 0 ? tmp : 1;
                    }

                    continue;
                }

                path.Segments.Add(segment);
            }

            static bool HasReachedTarget(Path path)
            {
                var lastSegment = path.Segments.Last();
                var isReached = (lastSegment.Direction.GeneralDirection()) switch
                {
                    Direction.Horizontal => path.EndPoint.Y == lastSegment.StartPoint.Y &&
                                            path.EndPoint.X >= Math.Min(lastSegment.StartPoint.X,
                                                lastSegment.EndPoint.X) &&
                                            path.EndPoint.X <= Math.Max(lastSegment.StartPoint.X,
                                                lastSegment.EndPoint.X),
                    _ => path.EndPoint.X == lastSegment.StartPoint.X &&
                         path.EndPoint.Y >= Math.Min(lastSegment.StartPoint.Y, lastSegment.EndPoint.Y) &&
                         path.EndPoint.Y <= Math.Max(lastSegment.StartPoint.Y, lastSegment.EndPoint.Y)
                };

                if (!isReached) return false;

                lastSegment.Length = (int)Point.Distance(lastSegment.StartPoint, path.EndPoint);
                return true;
            }

            Segment RandomSegment(Point start)
            {
                var len = RandomGenerator.Next(1, MaxLength + 1);
                var dir = RandomGenerator.Next(0, 2) switch
                {
                    0 => Direction.Horizontal,
                    _ => Direction.Vertical
                };

                // choose between going randomly and heading towards end point
                if (headToEndProb >= RandomGenerator.Next(0, 100))
                {
                    // head toward end point
                    var (x, y) = start;
                    if (dir == Direction.Horizontal)
                        dir = path.EndPoint.X >= x ? Direction.Right : Direction.Left;
                    else
                        dir = path.EndPoint.Y >= y ? Direction.Down : Direction.Up;
                }
                else
                {
                    // go randomly
                    dir = (dir, RandomGenerator.Next(0, 2)) switch
                    {
                        (Direction.Horizontal, 0) => Direction.Left,
                        (Direction.Horizontal, 1) => Direction.Right,
                        (Direction.Vertical, 0) => Direction.Down,
                        _ => Direction.Up
                    };

                    // increase probability to go towards end point next time
                    headToEndProb = Math.Min(headToEndProb + RandomGenerator.Next(0, 100), 100);
                }

                return new Segment(start, dir, len);
            }

            return path;
        }
    }
}