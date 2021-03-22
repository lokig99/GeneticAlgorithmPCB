using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class ShiftMutationAlpha : IMutationOperator
    {
        private int _maxShift = 1;
        private int _mutatedIndex;
        private int _nextIndex;
        private int _prevIndex;
        private Segment _mutatedSegment;
        private Segment _previousSegment;
        private Segment _nextSegment;

        public int MaxShift
        {
            get => _maxShift;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
                _maxShift = value;
            }
        }

        public Random RandomGenerator { get; set; } = new Random();

        public void Mutation(Solution solution, ISolutionInitializer initializer)
        {
            var index = RandomGenerator.Next(solution.Paths.Length);
            var path = solution.Paths[index];

            _mutatedIndex = RandomGenerator.Next(path.Segments.Count);
            _mutatedSegment = path.Segments[_mutatedIndex];
            _previousSegment = (_prevIndex = _mutatedIndex - 1) < 0
                ? null
                : path.Segments[_prevIndex];
            _nextSegment = (_nextIndex = _mutatedIndex + 1) >= path.Segments.Count
                ? null
                : path.Segments[_nextIndex];

            ShiftSegment(path);
        }

        private void ModifyTrailingSegment(Path path, Direction shiftDirection, int shift)
        {
            // trailing segment
            if (_previousSegment is null)
            {
                // create new segment and add to segment list
                _previousSegment = new Segment(path.StartPoint, shiftDirection, shift);
                path.Segments.Insert(0, _previousSegment);
                _prevIndex += 1;
                _nextIndex += 1;
                _mutatedIndex += 1;
            }
            else if (_previousSegment.Direction == _mutatedSegment.Direction.OppositeDirection())
            {
                _previousSegment = new Segment(_previousSegment.EndPoint, shiftDirection, shift);
                path.Segments.Insert(_mutatedIndex, _previousSegment);
                _nextIndex += 1;
                _mutatedIndex += 1;
            }
            else
            {
                var tmpLength = _previousSegment.Length +
                                (shiftDirection == _previousSegment.Direction ? shift : -shift);

                switch (tmpLength)
                {
                    case 0:
                        {
                            path.Segments.RemoveAt(_prevIndex);
                            _nextIndex -= 1;
                            _mutatedIndex -= 1;
                            _prevIndex -= 1;

                            if (_prevIndex < 0) return;

                            _previousSegment = path.Segments[_prevIndex];

                            // if there is another segment in same line and direction -> merge
                            if (_previousSegment.Direction != _mutatedSegment.Direction) return;
                            _mutatedSegment.StartPoint = _previousSegment.StartPoint;
                            _mutatedSegment.Length += _previousSegment.Length;

                            path.Segments.RemoveAt(_prevIndex);
                            _nextIndex -= 1;
                            _mutatedIndex -= 1;
                            break;
                        }
                    case < 0:
                        _previousSegment.Length = -tmpLength;
                        _previousSegment.Direction = _previousSegment.Direction.OppositeDirection();
                        break;
                    default:
                        _previousSegment.Length = tmpLength;
                        break;
                }
            }
        }

        private void ModifyFollowingSegment(Path path, Direction shiftDirection, int shift)
        {
            // following segment
            if (_nextSegment is null)
            {
                // create new segment and add to segment list
                _nextSegment = new Segment(_mutatedSegment.EndPoint, shiftDirection.OppositeDirection(), shift);
                path.Segments.Add(_nextSegment);
            }
            else if (_nextSegment.Direction == _mutatedSegment.Direction.OppositeDirection())
            {
                path.Segments.Insert(_nextIndex,
                    new Segment(_mutatedSegment.EndPoint, shiftDirection.OppositeDirection(), shift));
            }
            else
            {
                var tmpLength = _nextSegment.Length + (shiftDirection == _nextSegment.Direction ? -shift : shift);

                if (tmpLength == 0)
                {
                    path.Segments.RemoveAt(_nextIndex);

                    // if there are  no following segments then finish
                    if (_nextIndex >= path.Segments.Count) return;

                    _nextSegment = path.Segments[_nextIndex];


                    // if there is another segment in same line and direction -> merge
                    // ReSharper disable once InvertIf
                    if (_nextSegment.Direction == _mutatedSegment.Direction)
                    {
                        _mutatedSegment.Length += _nextSegment.Length;
                        path.Segments.RemoveAt(_nextIndex);
                    }
                }
                else
                {
                    _nextSegment.StartPoint = _mutatedSegment.EndPoint;
                    if (tmpLength < 0)
                    {
                        _nextSegment.Length = -tmpLength;
                        _nextSegment.Direction = _nextSegment.Direction.OppositeDirection();
                    }
                    else
                    {
                        _nextSegment.Length = tmpLength;
                    }
                }
            }
        }

        private void ShiftSegment(Path path)
        {
            var shift = RandomGenerator.Next(1, MaxShift + 1);
            var shiftDirection = (RandomGenerator.Next(2), _mutatedSegment.Direction.GeneralDirection()) switch
            {
                (0, Direction.Horizontal) => Direction.Up,
                (1, Direction.Horizontal) => Direction.Down,
                (0, Direction.Vertical) => Direction.Left,
                _ => Direction.Right
            };

            // move segment to new position
            var (x, y) = _mutatedSegment.StartPoint;
            _mutatedSegment.StartPoint = shiftDirection switch
            {
                Direction.Left => new Point(x - shift, y),
                Direction.Right => new Point(x + shift, y),
                Direction.Up => new Point(x, y - shift),
                _ => new Point(x, y + shift)
            };

            ModifyTrailingSegment(path, shiftDirection, shift);
            ModifyFollowingSegment(path, shiftDirection, shift);
            RepairPath(path);
        }

        public static void RepairPath(Path path)
        {
            var tmpSegments = new List<Segment>();
            var firstSegment = path.Segments.First();

            if (!path.StartPoint.Equals(firstSegment.StartPoint))
            {
                tmpSegments.AddRange(CreateConnection(path.StartPoint, firstSegment.StartPoint));
            }

            for (var i = 1; i < path.Segments.Count; i++)
            {
                var prevSegment = path.Segments[i - 1];
                var segment = path.Segments[i];
                tmpSegments.Add(prevSegment);

                if (prevSegment.EndPoint.Equals(segment.StartPoint)) continue;

                var newSegments = CreateConnection(prevSegment.EndPoint, segment.StartPoint);
                tmpSegments.AddRange(newSegments);
            }

            var lastSegment = path.Segments.Last();
            tmpSegments.Add(lastSegment);
            if (!lastSegment.EndPoint.Equals(path.EndPoint))
            {
                tmpSegments.AddRange(CreateConnection(lastSegment.EndPoint, path.EndPoint));
            }

            path.Segments = MergeRedundantSegments(tmpSegments);
        }

        public static List<Segment> MergeRedundantSegments(IReadOnlyList<Segment> segments)
        {
            var mergedSegments = new List<Segment>();
            var prevDir = segments[0].Direction;
            var prevIndex = 0;
            for (var i = 1; i < segments.Count; i++)
            {
                if (segments[i].Direction == prevDir) continue;

                var merged = segments[prevIndex];
                if (i - prevIndex > 1)
                {
                    // merge previous segments
                    for (var j = prevIndex + 1; j < i; j++)
                    {
                        merged.Length += segments[j].Length;
                    }
                }

                mergedSegments.Add(merged);
                prevDir = segments[i].Direction;
                prevIndex = i;
            }

            var last = segments[prevIndex];
            if (segments.Count - prevIndex > 1)
            {
                for (var i = prevIndex + 1; i < segments.Count; i++)
                {
                    last.Length += segments[i].Length;
                }
            }

            mergedSegments.Add(last);
            return mergedSegments;
        }

        public static IEnumerable<Segment> CreateConnection(Point startPoint, Point endPoint)
        {
            var (ex, ey) = endPoint;
            var dx = ex - startPoint.X;
            var dy = ey - startPoint.Y;
            var segments = new List<Segment>(2);

            if (dx != 0)
            {
                var xDir = dx > 0 ? Direction.Right : Direction.Left;
                dx = Math.Abs(dx);

                var xSegment = new Segment(startPoint, xDir, dx);
                segments.Add(xSegment);
                startPoint = xSegment.EndPoint;
            }

            if (dy == 0) return segments;

            var yDir = dy > 0 ? Direction.Down : Direction.Up;
            dy = Math.Abs(dy);

            var ySegment = new Segment(startPoint, yDir, dy);
            segments.Add(ySegment);
            return segments;
        }
    }
}