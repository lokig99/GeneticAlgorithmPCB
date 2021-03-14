using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithmPCB.GA.Interfaces;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA
{
    public readonly struct Point
    {
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
    }

    public class Segment : ICloneable<Segment>
    {
        private int _length;
        private Point _endPoint;
        private Point _startPoint;
        private Direction _direction;

        public Point StartPoint
        {
            get => _startPoint;
            set
            {
                _startPoint = value;
                _length = (int)Point.Distance(StartPoint, EndPoint);
            }
        }

        public Point EndPoint
        {
            get => _endPoint;
            set
            {
                _endPoint = value;
                _length = (int)Point.Distance(StartPoint, EndPoint);
            }
        }

        public Direction Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                _endPoint = CalculateEndPoint();
            }
        }

        public int Length
        {
            get => _length;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
                _length = value;
                EndPoint = CalculateEndPoint();
            }
        }

        public Segment(Point startPoint, Direction direction, int length)
        {
            _startPoint = startPoint;
            _direction = direction;
            _length = length;
            _endPoint = CalculateEndPoint();
        }

        private Point CalculateEndPoint()
        {
            return Direction switch
            {
                Direction.Left => new Point(StartPoint.X - Length, StartPoint.Y),
                Direction.Right => new Point(StartPoint.X + Length, StartPoint.Y),
                Direction.Up => new Point(StartPoint.X, StartPoint.Y - Length),
                _ => new Point(StartPoint.X, StartPoint.Y + Length)
            };
        }

        public Segment Clone()
        {
            return new Segment(StartPoint, Direction, Length);
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