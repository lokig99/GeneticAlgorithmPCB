using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneticAlgorithmPCB.GA
{
    public struct PcbProblem
    {
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }
        public List<(Point startPoint, Point endPoint)> PointPairs { get; set; }

        public PcbProblem(int boardWidth, int boardHeight, List<(Point startPoint, Point endPoint)> points)
        {
            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
            PointPairs = points;
        }

        public readonly bool IsPointOutside(Point p)
        {
            var (x, y) = p;
            return x < 0 || y < 0 || x >= BoardWidth || y >= BoardHeight;
        }

        public static PcbProblem LoadProblemFromFile(string path)
        {
            var points = new List<(Point startPoint, Point endPoint)>();
            int[] dimensions;
            using (var stream = File.OpenText(path))
            {
                // get pcb board dimensions
                dimensions = stream.ReadLine()?.Split(';').Select(int.Parse).ToArray();
                if (dimensions == null || dimensions.Length != 2)
                    throw new FormatException();

                // get points to connect
                while (!stream.EndOfStream)
                {
                    var pairs = stream.ReadLine()?.Split(';')
                        .Select((n, index) => new { PairNum = index / 2, num = int.Parse(n) })
                        .GroupBy(pair => pair.PairNum)
                        .Select(grp => grp.Select(g => g.num).ToArray()).ToArray();

                    if (pairs == null || pairs.Length != 2 || pairs[0].Length != 2) throw new FormatException();
                    var p1 = new Point(pairs[0][0], pairs[0][1]);
                    var p2 = new Point(pairs[1][0], pairs[1][1]);
                    points.Add((p1, p2));
                }
            }

            return new PcbProblem(dimensions[0], dimensions[1], points);
        }
    }
}