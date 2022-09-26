using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Helpers
{
    /// <summary>
    /// A helper to process every combination of two or more terms in an additive line.
    /// E.g. if you have "2x + 3y + 4z", it will do:
    /// (2x, 3y), (3y, 4z), (2x, 4z) and (2x, 3y, 4z)
    /// </summary>
    public struct AdditiveCombinationIterator
    {
        // TODO: Cache start position?
        AdditiveLine _line;
        bool _startedCurrSize = false;
        public int CurrentCombinationSize { get; set; }
        bool[] _currentCombination;

        public AdditiveCombinationIterator(AdditiveLine line)
        {
            _line = line;
            _currentCombination = new bool[line.Items.Count];
            CurrentCombinationSize = _currentCombination.Length;
            ResetDataForSize(_currentCombination.Length);
        }

        public bool NextCombinationSize()
        {
            if (!_startedCurrSize) return true;

            CurrentCombinationSize--;
            if (CurrentCombinationSize == 1) return false;

            ResetDataForSize(CurrentCombinationSize);
            return true;
        }

        public bool NextCombination()
        {
            if (!_startedCurrSize)
            {
                _startedCurrSize = true;
                return true;
            }

            if (CurrentCombinationSize == _currentCombination.Length) return false;

            // Example set of runs:
            // true, true, false, false
            // true, false, true, false
            // true, false, false, true
            // false, true, true, false
            // false, true, false, true
            // false, false, true, true

            int toResetAfter = 1;

            // Start from the end
            int currentPos = _currentCombination.Length;
            for (int i = 0; i < CurrentCombinationSize; i++)
            {
                // Move back to the previous currently selected item
                currentPos = Array.LastIndexOf(_currentCombination, true, currentPos - 1);
                if (currentPos == -1) throw new Exception("Something went very wrong!");

                // We're now at the last currently selected item.
                // If there's free space after it, simply move it along to the next position.
                if (currentPos != _currentCombination.Length - toResetAfter && !_currentCombination[currentPos + 1])
                {
                    _currentCombination[currentPos] = false;

                    // Set however many we need after
                    for (int j = 1; j <= toResetAfter; j++)
                        _currentCombination[currentPos + j] = true;

                    // All done
                    return true;
                }

                // Otherwise, if there's no more space, that means we're done with this current item, and need to iterate further back to the previous one
                else
                {
                    // Make sure this current item gets reset to after the previous one
                    _currentCombination[currentPos] = false;
                    toResetAfter++;
                }
            }

            // If we got here, that means we couldn't move any of the current items and are done with this size.
            return false;
        }

        public TreeItem GetFirst()
        {
            int first = Array.IndexOf(_currentCombination, true);
            if (first == -1) throw new Exception("Something went very wrong!");
            return _line.Items[first];
        }

        public void ResetToNoCopy(AdditiveCombination combination)
        {
            _currentCombination = combination.GetBoolArrayRepresentation();
            _startedCurrSize = false;
            CurrentCombinationSize = combination.CombinationSpread;
        }

        void ResetDataForSize(int size)
        {
            Array.Fill(_currentCombination, true, 0, size);
            Array.Fill(_currentCombination, false, size, _currentCombination.Length - size);
            _startedCurrSize = false;
        }

        public CurrentCombinationEnumerable EnumerateCurrent(bool skipFirst) => new(ref this, true, skipFirst);
        public CurrentCombinationEnumerable EnumerateNonCurrent(bool skipFirst) => new(ref this, false, skipFirst);
        public AdditiveCombination GetCurrentCombination()
        {
            bool[] newArr = new bool[_currentCombination.Length];
            Array.Copy(_currentCombination, newArr, _currentCombination.Length);
            return new AdditiveCombination(newArr, CurrentCombinationSize);
        }

        public struct CurrentCombinationEnumerable : IEnumerable<TreeItem>, IEnumerator<TreeItem>
        {
            AdditiveLine _line;
            bool _searchVal;
            int _currentPos;
            bool[] _currentCombination;

            public TreeItem Current => _line.Items[_currentPos];
            object IEnumerator.Current => _line.Items[_currentPos];

            public CurrentCombinationEnumerable(ref AdditiveCombinationIterator iter, bool searchVal, bool skipFirst)
            {
                _line = iter._line;
                _currentCombination = iter._currentCombination;
                _searchVal = searchVal;
                _currentPos = -1;

                // If we're skipping the first, move next now
                if (skipFirst) MoveNext();
            }

            public bool MoveNext()
            {
                _currentPos = Array.IndexOf(_currentCombination, _searchVal, _currentPos + 1);
                return _currentPos != -1;
            }

            public void Reset() => _currentPos = 0;

            public void Dispose() { }
            public IEnumerator<TreeItem> GetEnumerator() => this;
            IEnumerator IEnumerable.GetEnumerator() => this;
        }
    }
}
