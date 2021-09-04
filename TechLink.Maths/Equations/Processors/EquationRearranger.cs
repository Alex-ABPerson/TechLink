//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using TechLink.Core.Interfacing;
//using TechLink.Core.Models;
//using TechLink.Maths.Equations.Tree;

//namespace TechLink.Maths.Equations.Processors
//{
//    public static class EquationRearranger
//    {
//        // NOTE: A lot of algorithm is self-explanatory. It has two phases: Plan (analysis), Move.
//        // - In the planning phase, the algorithm finds where our variable (x) is in the tree, and tracks all the direct parents of that variable.
//        // - In the moving phase, the algorithm goes through each parent the "x" has (starting from the very top of the tree, outer) and "moves" them from one side to the other.
//        //   For instance, if we have "2x", the multiplication gets "moved" to the other side, causing a division by 2.
//        //   - There is one special scenario that can occur during this process. If "x" is found to be on the RIGHT of a division (2 / x), an operation called "Swap" is applied.
//        //     In this process, the contents of the other side are SWAPPED with the denominator. So, if you had "2 / 4x = 3", it would change into "2 / 3 = 4x".
//        //     After a "Swap", while the final trees have been heavily modified, the state does not have to change as all the remaining parents to be processed still match up correctly.
//        struct Parent
//        {
//            public TreeItem Item;
//            public int Index;

//            public bool IsVarRight => Index == 1;

//            public Parent(TreeItem item, int index) => (Item, Index) = (item, index);
//        }

//        struct State
//        {
//            public char TargetSubject;

//            // The side with the variable. The list starts from the variable at the beginning, the root as the end.
//            public List<Parent> VariableParents;
//            public int CurrentParentIndex;

//            public TreeItem VariableSide;
//            public TreeItem OppositeSide;

//            public State(char subject) => (VariableParents, CurrentParentIndex, VariableSide, OppositeSide, TargetSubject) = (new List<Parent>(), 0, null, null, subject);
//        }

//        public static void Rearrange(Equation equation, char subject)
//        {
//            var state = new State(subject);

//            // Analyze the tree to find where the variable is, and collect data about its parents.
//            Plan(equation, ref state);
//            LogPlanResults(state);

//            // Run the movement phase.
//            Interface.WriteLine("Starting movement process...");
//            ApplyRearrangement(ref state);
//            Interface.WriteLine("Rearrangement complete!");

//            equation.Left = state.VariableSide;
//            equation.Right = state.OppositeSide;
//        }

//        static void Plan(Equation equation, ref State state)
//        {
//            Interface.WriteLine("Analyzing to find variable location...");

//            if (TryAddVarParents(equation.Left, ref state)) 
//                (state.VariableSide, state.OppositeSide) = (equation.Left, equation.Right);

//            else if (TryAddVarParents(equation.Right, ref state))
//                (state.VariableSide, state.OppositeSide) = (equation.Right, equation.Left);

//            else throw new Exception("Unable to find variable.");

//            static bool TryAddVarParents(TreeItem currentItem, ref State state)
//            {
//                switch (currentItem)
//                {
//                    case Number variable:
//                        return false;
//                    case Variable variable:
//                        return variable.Name == state.TargetSubject;
//                    case AdditiveLine line:
//                        return TryAddVarSubItems(line.Items, ref state);
//                    case Function func:
//                        return TryAddVarSubItems(func.Arguments, ref state);
//                    case Root root:

//                        if (TryAddVarParents(root.Inner, ref state))
//                        {
//                            state.VariableParents.Add(new Parent(root.Inner, 0));
//                            return true;
//                        }
//                        else if (TryAddVarParents(root.RootPower, ref state))
//                        {
//                            state.VariableParents.Add(new Parent(root.RootPower, 1));
//                            return true;
//                        }
//                        return false;
//                    case Power power:

//                        if (TryAddVarParents(power.Base, ref state))
//                        {
//                            state.VariableParents.Add(new Parent(power.Base, 0));
//                            return true;
//                        }
//                        else if (TryAddVarParents(power.Exponent, ref state))
//                        {
//                            state.VariableParents.Add(new Parent(power.Exponent, 1));
//                            return true;
//                        }
//                        return false;
//                    default:
//                        throw new Exception("Unrecognized tree item");
//                }

//                static bool TryAddVarSubItems(IList<TreeItem> items, ref State state)
//                {
//                    for (int i = 0; i < items.Count; i++)
//                        if (TryAddVarParents(items[i], ref state))
//                        {
//                            state.VariableParents.Add(new Parent(items[i], i));
//                            return true;
//                        }

//                    return false;
//                }
//            }
//        }

//        static void ApplyRearrangement(ref State state)
//        {
//            for (int i = state.VariableParents.Count - 1; i >= 0; i--)
//            {
//                Interface.Write("Moving the following parent: ");
//                Interface.WriteLine(state.VariableParents[i].Item.ToString());
//                Move(state.VariableParents[i], ref state);
//            }
//        }

//        static void Move(Parent itemInfo, ref State state)
//        {
//            switch (itemInfo.Item)
//            {
//                case Number:
//                case Variable:

//                    throw new Exception("Cannot move single item");

//                case AdditiveLine line:

//                    for (int i = 0; i < line.Items.Count; i++)
//                    {
//                        if (i == itemInfo.Index) continue;
//                        EmplaceAdditive(line.Items[i], ref state);
//                    }

//                    state.VariableSide = line.Items[itemInfo.Index];
//                    break;

//                case TermLine term:

//                    for (int i = 0; i < term.Terms.Count; i++)
//                    {
//                        if (i == itemInfo.Index) continue;
//                        EmplaceDivide(term.Terms[i], ref state);
//                    }

//                    state.VariableSide = term.Terms[itemInfo.Index];
//                    break;

//                case Root root:

//                    if (itemInfo.Index == 1)
//                        throw new Exception("Variable was found in root");

//                    Emplace(new Power(state.OppositeSide, root.RootPower), ref state);
//                    state.VariableSide = root.Inner;
//                    break;

//                case Function func:

//                    state.VariableSide = func.Arguments[0];

//                    func.IsInverse = !func.IsInverse;
//                    func.Arguments[0] = state.OppositeSide;
//                    Emplace(func, ref state);

//                    break;

//                case Power power:

//                    if (itemInfo.Index == 1)
//                        throw new Exception("Variable was found in the exponent of a power - log is not currently supported!");

//                    state.VariableSide = power.Exponent;
//                    power.Base = 
//                    func.Arguments[0] = state.OppositeSide;
//                    Emplace(func, ref state);

//                    break;
//            }

//            string str = "Hello world!";
//            str.StringExtension<int>();
//        }

//        static void StringExtension<T>(this object itm)
//        {

//        }

//        private static void MoveAdditive(Parent itemInfo, ref State state, AdditiveLine line)
//        {
            
//        }

//        static void MoveTerm(Parent itemInfo, ref State state, AdditiveLine line)
//        {
//            int oldSize = line.Items.Count;

//            // Move all the items before the variable.
//            for (int i = 0; i < itemInfo.Index; i++)
//            {
//                line.Items[0].IsNegative = !line.Items[0].IsNegative;
//                EmplaceDivide(line.Items[0], ref state);
//                line.Items.RemoveAt(0);
//            }

//            // Move all the items after the variable.
//            for (int i = itemInfo.Index + 1; i < oldSize; i++)
//            {
//                line.Items[1].IsNegative = !line.Items[1].IsNegative;
//                EmplaceDivide(line.Items[1], ref state);
//                line.Items.RemoveAt(1);
//            }
//        }

//        static void MoveOperator(bool isVarRight, ref State state)
//        {
//            var varSide = isVarRight ? operation.Right : operation.Left;
//            var nonVarSide = isVarRight ? operation.Left : operation.Right;

//            // Add the correct operation to the other side.
//            switch (operation.Type)
//            {
//                case OperatorTreeItemType.Add:

//                    Negate(ref nonVarSide);
//                    Emplace(new OperatorTreeItem(OperatorTreeItemType.Add, state.OppositeSide, nonVarSide), ref state);

//                    break;
//                case OperatorTreeItemType.Multiply:
//                    Emplace(new OperatorTreeItem(OperatorTreeItemType.Divide, state.OppositeSide, nonVarSide), ref state);
//                    break;

//                case OperatorTreeItemType.Divide:

//                    if (isVarRight)
//                    {
//                        Interface.WriteLine("Variable is on right of division... A swap must be done!");
                        
//                        operation.Right = state.OppositeSide;
//                        state.VariableSide = varSide;
//                        state.OppositeSide = operation;
//                        return;
//                    }

//                    Emplace(new OperatorTreeItem(OperatorTreeItemType.Multiply, state.OppositeSide, nonVarSide), ref state);
//                    break;
//                case OperatorTreeItemType.PowerOf:

                  

//                    break;
//            }

//            // Remove the operation from the source tree.
//            state.VariableSide = varSide;
//        }

//        static void LogPlanResults(State state)
//        {
//            Interface.Write("Analysis found the variable contained within these operations: ");
//            for (int i = state.VariableParents.Count - 1; i >= 0; i--)
//            {
//                Interface.Write(state.VariableParents[i].Item.ToString());

//                if (i > 0) Interface.Write(", ");
//            }

//            Interface.WriteLine();
//        }

//        static void Emplace(TreeItem item, ref State state) => state.OppositeSide = item;

//        static void EmplaceAdditive(TreeItem item, ref State state)
//        {
//            item.IsNegative = !item.IsNegative;

//            if (state.OppositeSide is AdditiveLine line)
//                line.Items.Add(item);
//            else
//                Emplace(new AdditiveLine(state.OppositeSide, item), ref state);
//        }

//        static void EmplaceDivide(TreeItem item, ref State state)
//        {
//            // If there's already a division, place it into the denominator as a multiplication.
//            if (state.OppositeSide is Division division)
//            {
//                if (division.Bottom is TermLine term)
//                {
//                    term.Terms.Add(item);
//                }
//                else
//                {
//                    division.Bottom = new TermLine(division.Bottom, item);
//                }
//            }
//            else
//            {
//                Emplace(new Division(state.OppositeSide, division.Bottom), ref state);
//            }
//        }
//    }
//}
