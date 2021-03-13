using System.Collections.Generic;
using GeneticAlgorithmPCB.GA;
using Xunit;

namespace GeneticAlgorithmPCB.Testing
{
    public class SolutionTest
    {
        private readonly PcbProblem _problem;

        public SolutionTest()
        {
            var points = new List<(Point startPoint, Point endPoint)>
            {
                (new Point(1,3), new Point(5,3)),
                (new Point(3,1), new Point(3,3))
            };

            _problem = new PcbProblem(6, 6, points);
        }


        [Fact]
        public void IsIntersectionCountValid_NoIntersections_ReturnsZero()
        {
            // prepare test 
            var paths = new Path[2];
            var (sp1, ep1) = _problem.PointPairs[0];
            var (sp2, ep2) = _problem.PointPairs[1];
            var path1 = paths[0] = new Path(sp1, ep1);
            path1.Segments = new List<Segment>
            {
                new Segment(new Point(1, 3), Direction.Down, 1),
                new Segment(new Point(1, 4), Direction.Right, 4),
                new Segment(new Point(5, 4), Direction.Up, 1)
            };

            var path2 = paths[1] = new Path(sp2, ep2);
            path2.Segments = new List<Segment>
            {
                new Segment(new Point(3, 1), Direction.Down, 2)
            };


            var solution = new Solution(_problem, paths);

            // check
            Assert.Equal(0, solution.Intersections);
        }

        [Fact]
        public void IsIntersectionCountValid_TwoIntersections_ReturnsTwo()
        {
            // prepare test 
            var paths = new Path[2];
            var (sp1, ep1) = _problem.PointPairs[0];
            var (sp2, ep2) = _problem.PointPairs[1];
            var path1 = paths[0] = new Path(sp1, ep1);
            path1.Segments = new List<Segment>
            {
                new Segment(new Point(1, 3), Direction.Right, 4)
            };

            var path2 = paths[1] = new Path(sp2, ep2);
            path2.Segments = new List<Segment>
            {
                new Segment(new Point(3, 1), Direction.Left, 1),
                new Segment(new Point(2, 1), Direction.Down, 3),
                new Segment(new Point(2, 4), Direction.Right, 1),
                new Segment(new Point(3, 4), Direction.Up, 1)
            };


            var solution = new Solution(_problem, paths);

            // check
            Assert.Equal(2, solution.Intersections);
        }

        [Fact]
        public void IsSegmentsOutsideBoardCountValid_ThreeSegmentsOutside_ReturnsThree()
        {
            // prepare test 
            var paths = new Path[1];
            var (sp1, ep1) = _problem.PointPairs[0];
            var path1 = paths[0] = new Path(sp1, ep1);
            path1.Segments = new List<Segment>
            {
                new Segment(new Point(1, 3), Direction.Down, 1),
                new Segment(new Point(1, 4), Direction.Right, 5),
                new Segment(new Point(6, 4), Direction.Up, 1),
                new Segment(new Point(6, 3), Direction.Left, 1)
            };

            var solution = new Solution(_problem, paths);

            //check
            Assert.Equal(3, solution.SegmentsOutsideBoardStats.count);
        }

        [InlineData(0, 0, false)]
        [InlineData(-1, -1, true)]
        [InlineData(3, 4, false)]
        [InlineData(6, 0, true)]
        [InlineData(6, 6, true)]
        [InlineData(0, 6, true)]
        [InlineData(3, 10, true)]
        [InlineData(-1, 10, true)]
        [InlineData(-1, 0, true)]
        [InlineData(0, -1, true)]
        [InlineData(6, -1, true)]
        [InlineData(6, 7, true)]
        [Theory]
        public void IsPointOutsideBoard(int x, int y, bool expected)
        {
            var p = new Point(x, y);
            Assert.Equal(expected, _problem.IsPointOutside(p));
        }

        [Fact]
        public void TotalLengthOfFragmentsOutsideBoard_ThreeSegmentsOutside_ReturnsThree()
        {
            // prepare test 
            var paths = new Path[1];
            var (sp1, ep1) = _problem.PointPairs[0];
            var path1 = paths[0] = new Path(sp1, ep1);
            path1.Segments = new List<Segment>
            {
                new Segment(new Point(1, 3), Direction.Down, 1),
                new Segment(new Point(1, 4), Direction.Right, 5),
                new Segment(new Point(6, 4), Direction.Up, 1),
                new Segment(new Point(6, 3), Direction.Left, 1)
            };

            var solution = new Solution(_problem, paths);

            //check
            Assert.Equal(3, solution.SegmentsOutsideBoardStats.length);
        }
    }
}


