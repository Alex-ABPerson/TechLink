using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Helpers
{
    internal struct TreeExplorer
    {
        List<ItemState> _currentState;

        public TreeExplorer(TreeItem item, bool excludePrimitives)
        {
            _currentState = new List<ItemState>(8);
            PushState(new ItemState(item));
        }

        public void IterateUp(Action<bool, TreeItem> handler)
        {
            while (_currentState.Count > 0)
            {
                while (MoveNextDown());
                handler(_currentState.Count == 1, PeekState().Item);
                PopState();
            }
        }

        bool MoveNextDown()
        {
            ItemState curr = PeekState();

            switch (curr.Item)
            {
                case Number:
                case Variable:
                    return false;
                case TermLine line:
                    if (line.Terms.Count == ++curr.Pos) return false;
                    PushState(new ItemState(line.Terms[curr.Pos]));
                    goto UpdatedCurrAndPushed;

                case AdditiveLine line:
                    if (line.Items.Count == ++curr.Pos) return false;
                    PushState(new ItemState(line.Items[curr.Pos]));
                    goto UpdatedCurrAndPushed;

                case Function func:
                    if (func.Arguments.Count == ++curr.Pos) return false;
                    PushState(new ItemState(func.Arguments[curr.Pos]));
                    goto UpdatedCurrAndPushed;

                case Division division:
                    if (curr.Pos == 2) return false;
                    PushState(new ItemState(++curr.Pos == 0 ? division.Top : division.Top));
                    goto UpdatedCurrAndPushed;

                case Root root:
                    if (curr.Pos == 2) return false;
                    PushState(new ItemState(++curr.Pos == 0 ? root.Inner : root.Index));
                    goto UpdatedCurrAndPushed;

                case Power power:
                    if (curr.Pos == 2) return false;
                    PushState(new ItemState(++curr.Pos == 0 ? power.Base : power.Exponent));
                    goto UpdatedCurrAndPushed;

                default:
                    throw new Exception("Unsupported tree item");
            }

        UpdatedCurrAndPushed:
            _currentState[^2] = curr;
            return true;
        }

        public void SetCurrentItemInTree(TreeItem tree, TreeItem newValue)
        {
            if (_currentState.Count == 1) throw new Exception("Can't assign child of a single-item tree.");

            // Iterate through the given tree towards the item.
            TreeItem current = tree;
            for (int i = 0; i < _currentState.Count - 2; i++)
            {
                int currentPos = _currentState[i].Pos;

                current = current switch
                {
                    Number => throw new Exception("Cannot assign child of number."),
                    Variable => throw new Exception("Cannot assign child of variable."),
                    TermLine line => line.Terms[currentPos],
                    AdditiveLine line => line.Items[currentPos],
                    Function func => func.Arguments[currentPos],
                    Division division => currentPos == 0 ? division.Top : division.Bottom,
                    Root root => currentPos == 0 ? root.Inner : root.Index,
                    Power power => currentPos == 0 ? power.Base : power.Exponent,
                    _ => throw new Exception("Unsupported tree item")
                };
            }

            // Assign the item in the tree
            int itmPos = _currentState[_currentState.Count - 2].Pos;
            switch (current)
            {
                case Number:
                case Variable:
                    throw new Exception("Cannot assign child of number or variable.");
                case TermLine line:
                    line.Terms[itmPos] = newValue;
                    break;
                case AdditiveLine line:
                    line.Items[itmPos] = newValue;
                    break;
                case Function func:
                    func.Arguments[itmPos] = newValue;
                    break;
                case Division division:
                    if (itmPos == 0)
                        division.Top = newValue;
                    else
                        division.Bottom = newValue;
                    
                    break;
                case Root root:
                    if (itmPos == 0)
                        root.Inner = newValue;
                    else
                        root.Index = newValue;

                    break;
                case Power power:
                    if (itmPos == 0)
                        power.Base = newValue;
                    else
                        power.Exponent = newValue;

                    break;
                default:
                    throw new Exception("Unsupported tree item");
            }

        }

        void PushState(ItemState newState) => _currentState.Add(newState);
        ItemState PopState()
        {
            ItemState last = PeekState();
            _currentState.RemoveAt(_currentState.Count - 1);
            return last;
        }

        ItemState PeekState() => _currentState[^1];

        //int FindDeepestLayer()
        //{
        //    int layer = 0;
        //    int maxLayer = 0;
            
        //    while (_currentState.Count > 0)
        //    {
        //        while (MoveDownNext())
        //            layer++;

        //        PopState();
        //        maxLayer = layer--;
        //    }

        //    return maxLayer;
        //}

        struct ItemState
        {
            public TreeItem Item;
            public int Pos;

            public ItemState(TreeItem item, int pos = -1) => (Item, Pos) = (item, pos);
        }
    }
}
