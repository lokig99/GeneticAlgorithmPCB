using System;
using System.Linq;

namespace GeneticAlgorithmPCB.GA.Operators.Initialization
{
    public class RandomSolutionInitializer : ISolutionInitializer
    {
        public void Initialize(in Solution solution, int? seed = null)
        {
            solution.Paths = new Path[solution.Problem.PointPairs.Count];

            var rand = seed is null ? new Random() : new Random((int)seed);
            const int maxLength = 5;

            for (var i = 0; i < solution.Paths.Length; i++)
            {
                var (startPoint, endPoint) = solution.Problem.PointPairs[i];
                var path = solution.Paths[i] = new Path(startPoint, endPoint);
                var headToEndProb = rand.Next(0, 100);
                path.Segments.Add(RandomSegment(startPoint, path, ref headToEndProb));

                while (!HasReachedTarget(path))
                {
                    var prevSegment = path.Segments.Last();
                    var segment = RandomSegment(prevSegment.EndPoint, path, ref headToEndProb);

                    if (GeneralDirection(segment.Direction) == GeneralDirection(prevSegment.Direction))
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
            }

            Segment RandomSegment(Point startPoint, Path path, ref int headToEndProb)
            {
                var len = rand.Next(1, maxLength + 1);
                var dir = rand.Next(0, 2) switch
                {
                    0 => Direction.Horizontal,
                    _ => Direction.Vertical
                };

                // choose between going randomly and heading towards end point
                if (headToEndProb >= rand.Next(0, 100))
                {
                    // head toward end point
                    if (dir == Direction.Horizontal)
                        dir = path.EndPoint.X >= startPoint.X ? Direction.Right : Direction.Left;
                    else
                        dir = path.EndPoint.Y >= startPoint.Y ? Direction.Down : Direction.Up;
                }
                else
                {
                    // go randomly
                    dir = (dir, rand.Next(0, 2)) switch
                    {
                        (Direction.Horizontal, 0) => Direction.Left,
                        (Direction.Horizontal, 1) => Direction.Right,
                        (Direction.Vertical, 0) => Direction.Down,
                        _ => Direction.Up
                    };

                    // increase probability to go towards end point next time
                    headToEndProb = Math.Min(headToEndProb + rand.Next(0, 100), 100);
                }

                return new Segment(startPoint, dir, len);
            }

            static Direction GeneralDirection(Direction dir)
            {
                return dir switch
                {
                    Direction.Left => Direction.Horizontal,
                    Direction.Right => Direction.Horizontal,
                    _ => Direction.Vertical
                };
            }

            static bool HasReachedTarget(Path path)
            {
                var lastSegment = path.Segments.Last();
                var isReached = GeneralDirection(lastSegment.Direction) switch
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
                lastSegment.EndPoint = path.EndPoint;
                return true;
            }
        }
    }
}