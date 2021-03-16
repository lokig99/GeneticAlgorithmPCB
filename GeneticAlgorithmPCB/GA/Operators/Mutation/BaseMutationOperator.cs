using System;
using System.Reflection.Metadata.Ecma335;
using GeneticAlgorithmPCB.GA.Operators.Initialization;

namespace GeneticAlgorithmPCB.GA.Operators.Mutation
{
    public class BaseMutationOperator : IMutationOperator
    {
        private double _mutationChance = 0.5;
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

        public double MutationChance
        {
            get => _mutationChance;
            set
            {
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException(nameof(value));
                _mutationChance = value;
            }
        }

        public Random RandomGenerator { get; set; } = new Random();

        public void Mutation(Solution solution, ISolutionInitializer initializer)
        {
            for (var index = 0; index < solution.Paths.Length; index++)
            {
                var path = solution.Paths[index];
                if (MutationChance < RandomGenerator.NextDouble()) continue;

                _mutatedIndex = RandomGenerator.Next(path.Segments.Count);
                _mutatedSegment = path.Segments[_mutatedIndex];
                _previousSegment = (_prevIndex = _mutatedIndex - 1) < 0
                    ? null
                    : path.Segments[_prevIndex];
                _nextSegment = (_nextIndex = _mutatedIndex + 1) >= path.Segments.Count
                    ? null
                    : path.Segments[_nextIndex];

                if (ShiftSegment(path)) continue;
                var newPath = initializer.GeneratePath(solution, index);
                solution.Paths[index] = newPath;
            }
        }

        private bool ModifyTrailingSegment(Path path, Direction shiftDirection, int shift)
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

                if (tmpLength == 0)
                {
                    path.Segments.RemoveAt(_prevIndex);
                    _nextIndex -= 1;
                    _mutatedIndex -= 1;
                    _prevIndex -= 1;

                    if (_prevIndex >= 0)
                    {
                        _previousSegment = path.Segments[_prevIndex];

                        //bad
                        if (!_previousSegment.EndPoint.Equals(_mutatedSegment.StartPoint))
                        {
                            //  Console.WriteLine("shouldn't happen :<");
                            return false;
                        }

                        // if there is another segment in same line and direction -> merge
                        if (_previousSegment.Direction == _mutatedSegment.Direction)
                        {
                            _mutatedSegment.StartPoint = _previousSegment.StartPoint;
                            _mutatedSegment.Length += _previousSegment.Length;

                            path.Segments.RemoveAt(_prevIndex);
                            _nextIndex -= 1;
                            _mutatedIndex -= 1;
                        }
                    }
                }
                else if (tmpLength < 0)
                {
                    _previousSegment.Length = -tmpLength;
                    _previousSegment.Direction = _previousSegment.Direction.OppositeDirection();
                }
                else
                {
                    _previousSegment.Length = tmpLength;
                }
            }

            return true;
        }

        private bool ModifyFollowingSegment(Path path, Direction shiftDirection, int shift)
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
                    if (_nextIndex >= path.Segments.Count) return true;

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

            return true;
        }

        private bool ShiftSegment(Path path)
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

            return ModifyFollowingSegment(path, shiftDirection, shift) &&
                   ModifyTrailingSegment(path, shiftDirection, shift) && ValidatePath(path);
        }

        private static bool ValidatePath(Path path)
        {
            for (var i = 1; i < path.Segments.Count - 1; i++)
            {
                var prevSegment = path.Segments[i - 1];
                var segment = path.Segments[i];

                if (!segment.StartPoint.Equals(prevSegment.EndPoint))
                    return false;
            }

            return true;
        }
    }
}