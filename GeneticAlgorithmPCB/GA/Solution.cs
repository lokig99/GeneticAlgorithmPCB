using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithmPCB.GA.Interfaces;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA
{
    public readonly struct Point
    {
        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Point other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public readonly int X, Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public static double Distance(Point p1, Point p2)
        {
            var (x, y) = p1;
            var (x2, y2) = p2;
            return Math.Sqrt(Math.Pow(x - x2, 2) + Math.Pow(y - y2, 2));
        }

        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }
    }

    public class Segment : ICloneable<Segment>
    {
        private int _length;

        public Point StartPoint { get; set; }

        public Point EndPoint =>
            Direction switch
            {
                Direction.Left => new Point(StartPoint.X - Length, StartPoint.Y),
                Direction.Right => new Point(StartPoint.X + Length, StartPoint.Y),
                Direction.Up => new Point(StartPoint.X, StartPoint.Y - Length),
                _ => new Point(StartPoint.X, StartPoint.Y + Length)
            };

        public Direction Direction { get; set; }

        public int Length
        {
            get => _length;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
                _length = value;
            }
        }

        public Segment(Point startPoint, Direction direction, int length)
        {
            StartPoint = startPoint;
            Direction = direction;
            _length = length;
        }

        public Segment Clone()
        {
            return new Segment(StartPoint, Direction, Length);
        }

        public override string ToString()
        {
            return $"{StartPoint} -> {EndPoint} {Direction} L={Length}";
        }
    }

    public class Path : ICloneable<Path>
    {
        public Point StartPoint { get; }
        public Point EndPoint { get; }
        public List<Segment> Segments { get; set; }

        public Path(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Segments = new List<Segment>();
        }

        private Path(Point startPoint, Point endPoint, List<Segment> segments)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Segments = segments;
        }

        public Path Clone()
        {
            var segmentsClones = Segments.ConvertAll(s => s.Clone());
            return new Path(StartPoint, EndPoint, segmentsClones);
        }
    }

    public enum Direction
    {
        Left,
        Right,
        Up,
        Down,
        Vertical,
        Horizontal
    }

    internal static class DirectionMethods
    {
        public static Direction OppositeDirection(this Direction dir)
        {
            return dir switch
            {
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                _ => dir
            };
        }

        public static Direction GeneralDirection(this Direction dir)
        {
            return dir switch
            {
                Direction.Left or Direction.Right => Direction.Horizontal,
                Direction.Up or Direction.Down => Direction.Vertical,
                _ => dir
            };
        }

        public static (int dx, int dy) CoefficientTuple(this Direction dir)
        {
            return dir switch
            {
                Direction.Up => (0, -1),
                Direction.Down => (0, 1),
                Direction.Left => (-1, 0),
                Direction.Right => (1, 0),
                _ => (0, 0)
            };
        }
    }

    public class Solution : ICloneable<Solution>
    {
        public Path[] Paths { get; set; }
        public int TotalLength => Paths.Sum(p => p.Segments.Sum(s => s.Length));
        public int TotalSegmentCount => Paths.Sum(p => p.Segments.Count);
        public int Intersections => GetIntersectionsCount();
        public (int count, int length) SegmentsOutsideBoardStats => GetSegmentsOutsideBoardStats();
        public PcbProblem Problem { get; set; }


        public Solution(PcbProblem problem)
        {
            Paths = new Path[problem.PointPairs.Count];
            Problem = problem;
        }

        public Solution(PcbProblem problem, ISolutionInitializer initializer) : this(problem)
        {
            ResetSolution(initializer);
        }

        public Solution(PcbProblem problem, Path[] paths)
        {
            Paths = paths;
            Problem = problem;
        }


        public void ResetSolution(ISolutionInitializer initializer)
        {
            initializer.Initialize(this);
        }

        private IEnumerable<Segment> GetSegmentsOutsideBoard()
        {
            return Paths.SelectMany(p => p.Segments).Where(IsOutside);

            bool IsOutside(Segment s)
            {
                return Problem.IsPointOutside(s.StartPoint) ||
                       Problem.IsPointOutside(s.EndPoint);
            }
        }


        private (int count, int length) GetSegmentsOutsideBoardStats()
        {
            var segments = GetSegmentsOutsideBoard().ToArray();
            return (segments.Length, segments.Sum(LengthOutside));

            int LengthOutside(Segment s)
            {
                return
                    (Problem.IsPointOutside(s.StartPoint) &&
                     Problem.IsPointOutside(s.EndPoint)) switch
                    {
                        true => s.Length,
                        _ => GetFragmentLength(s)
                    };
            }

            int GetFragmentLength(Segment s)
            {
                return (Dir: s.Direction, Problem.IsPointOutside(s.StartPoint)) switch
                {
                    (Direction.Up, true) => s.StartPoint.Y - (Problem.BoardHeight - 1),
                    (Direction.Up, false) => 0 - s.EndPoint.Y,
                    (Direction.Down, true) => 0 - s.StartPoint.Y,
                    (Direction.Down, false) => s.EndPoint.Y - (Problem.BoardHeight - 1),
                    (Direction.Left, true) => s.StartPoint.X - (Problem.BoardWidth - 1),
                    (Direction.Left, false) => 0 - s.EndPoint.X,
                    (Direction.Right, true) => 0 - s.StartPoint.X,
                    _ => s.EndPoint.X - (Problem.BoardWidth - 1)
                };
            }
        }

        private int GetIntersectionsCount()
        {
            var pointsCounter = new Dictionary<Point, int>();

            foreach (var path in Paths)
            {
                // include path's start point
                pointsCounter[path.StartPoint] = pointsCounter.GetValueOrDefault(path.StartPoint, -1) + 1;

                foreach (var s in path.Segments)
                {
                    var (sx, sy) = s.StartPoint;
                    var (dx, dy) = s.Direction switch
                    {
                        Direction.Up => (0, -1),
                        Direction.Down => (0, 1),
                        Direction.Left => (-1, 0),
                        _ => (1, 0)
                    };


                    for (var i = 1; i <= s.Length; i++)
                    {
                        var tmp = new Point(sx + dx * i, sy + dy * i);
                        pointsCounter[tmp] = pointsCounter.GetValueOrDefault(tmp, -1) + 1;
                    }
                }
            }

            return pointsCounter.Values.Sum();
        }

        public Solution Clone()
        {
            return new Solution(Problem, Paths.Select(p => p.Clone()).ToArray());
        }
    }
}