using System;
using System.Collections.Generic;
using System.Linq;

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
    }

    public struct Segment
    {
        public Point StartPoint { get; set; }

        public Point EndPoint => Dir switch
        {
            Direction.Left => new Point(StartPoint.X - Length, StartPoint.Y),
            Direction.Right => new Point(StartPoint.X + Length, StartPoint.Y),
            Direction.Up => new Point(StartPoint.X, StartPoint.Y - Length),
            _ => new Point(StartPoint.X, StartPoint.Y + Length)
        };

        public Direction Dir { get; set; }
        public int Length { get; set; }

        public Segment(Point startPoint, Direction dir, int length)
        {
            StartPoint = startPoint;
            Dir = dir;
            Length = length;
        }
    }

    public class Path
    {
        public Point StartPoint { get; }
        public Point EndPoint { get; }
        public LinkedList<Segment> Segments { get; }

        public Path(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Segments = new LinkedList<Segment>();
        }
    }

    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    public class Solution
    {
        public Path[] Paths { get; set; }

        public int TotalLength => Paths.Sum(p => p.Segments.Sum(s => s.Length));
        public int TotalSegmentCount => Paths.Sum(p => p.Segments.Count);
        public int Intersections => GetIntersectionsCount();


        public Solution(Path[] paths)
        {
            Paths = paths;
        }

        private IEnumerable<Segment> GetSegmentsOutsideBoard(int boardWidth, int boardHeight)
        {
            return Paths.SelectMany(p => p.Segments).Where(IsOutside);

            bool IsOutside(Segment s)
            {
                return IsPointOutside(s.StartPoint, boardWidth, boardHeight) ||
                       IsPointOutside(s.EndPoint, boardWidth, boardHeight);
            }
        }

        private static bool IsPointOutside(Point p, int boardWidth, int boardHeight)
        {
            var (x, y) = p;
            return x < 0 || y < 0 || x > boardWidth || y > boardHeight;
        }

        public (int count, int length) SegmentsOutsideBoardStats(int boardWidth, int boardHeight)
        {
            var segments = GetSegmentsOutsideBoard(boardWidth, boardHeight).ToArray();
            return (segments.Length, segments.Sum(LengthOutside));

            int LengthOutside(Segment s)
            {
                return
                    (IsPointOutside(s.StartPoint, boardWidth, boardHeight) &&
                     IsPointOutside(s.EndPoint, boardWidth, boardHeight)) switch
                    {
                        true => s.Length,
                        _ => GetFragmentLength(s)
                    };
            }

            int GetFragmentLength(Segment s)
            {
                return (s.Dir, IsPointOutside(s.StartPoint, boardWidth, boardHeight)) switch
                {
                    (Direction.Up, true) => s.StartPoint.Y - boardHeight,
                    (Direction.Up, false) => 0 - s.EndPoint.Y,
                    (Direction.Down, true) => 0 - s.StartPoint.Y,
                    (Direction.Down, false) => s.EndPoint.Y - boardHeight,
                    (Direction.Left, true) => s.StartPoint.X - boardWidth,
                    (Direction.Left, false) => 0 - s.EndPoint.X,
                    (Direction.Right, true) => 0 - s.StartPoint.X,
                    _ => s.EndPoint.X - boardHeight
                };
            }
        }

        private int GetIntersectionsCount()
        {
            var pointsCounter = new Dictionary<Point, int>();

            var segments = Paths.SelectMany(p => p.Segments);
            foreach (var s in segments)
            {
                var (sx, sy) = s.StartPoint;
                var (dx, dy) = s.Dir switch
                {
                    Direction.Up => (0, -1),
                    Direction.Down => (0, 1),
                    Direction.Left => (-1, 0),
                    _ => (1, 0)
                };

                for (var i = 0; i <= s.Length; i++)
                {
                    var tmp = new Point(sx + dx * i, sy + dy * i);
                    pointsCounter[tmp] = pointsCounter.GetValueOrDefault(tmp, -1) + 1;
                }
            }

            return pointsCounter.Values.Sum();
        }
    }
}