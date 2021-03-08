using System;
using System.Linq;
using GeneticAlgorithmPCB.GA.Interfaces;

namespace GeneticAlgorithmPCB.GA
{
    public class RandomPopInitializer : IPopulationInitializer
    {
        public Solution[] Initialize(PcbProblem problem, int populationSize)
        {
            var population = new Solution[populationSize];

            for (var i = 0; i < population.Length; i++)
            {
                var paths = new Path[problem.PointPairs.Count];
                for (var j = 0; j < paths.Length; j++)
                {
                    paths[j] = new Path(problem.PointPairs[j].startPoint, problem.PointPairs[j].endPoint);
                    var path = paths[j];

                    do
                    {
                        var segment =
                            DrawRandomSegment(path.Segments.Count > 0 ? path.Segments.Last().EndPoint : path.StartPoint,
                                5);

                        if (path.Segments.Count == 0)
                        {
                            segment.Dir = path.StartPoint.Y > path.EndPoint.Y ? Direction.Up : Direction.Down;
                        }
                        else
                        {
                            var prevSeg = path.Segments.Last();
                            if (prevSeg.Dir == Direction.Down || prevSeg.Dir == Direction.Up)
                            {
                                segment.Dir = segment.StartPoint.X > path.EndPoint.X ? Direction.Left : Direction.Right;
                                var sEndPoint = segment.EndPoint;
                                if (sEndPoint.Y == path.EndPoint.Y)
                                {
                                    segment.Length = Math.Abs(path.EndPoint.X - segment.StartPoint.X);
                                }
                            }
                            else
                            {
                                segment.Dir = segment.StartPoint.Y > path.EndPoint.Y ? Direction.Up : Direction.Down;
                                var sEndPoint = segment.EndPoint;
                                if (sEndPoint.X == path.EndPoint.X)
                                {
                                    segment.Length = Math.Abs(path.EndPoint.Y - segment.StartPoint.Y);
                                }
                            }
                        }

                        path.Segments.Add(segment);
                    } while (!Equals(path.Segments.Last().EndPoint, path.EndPoint));
                }

                population[i] = new Solution(paths);
            }

            return population;

            static Segment DrawRandomSegment(Point startPoint, int maxLength)
            {
                var rand = new Random();
                var len = rand.Next(1, maxLength + 1);
                // var dir = rand.Next(0, 2) switch
                // {
                //     0 => Direction.Up,
                //     1 => Direction.Down,
                //     2 => Direction.Right,
                //     _ => Direction.Left
                // };

                return new Segment(startPoint, Direction.Down, len);
            }
        }
    }
}