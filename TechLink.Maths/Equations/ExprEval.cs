using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations
{
    /// <summary>
    /// Performs operations on the exact tree item given.
    /// NOTE: None of these are recursive, meaning none of them touch the INNER items of the given trees, they only touch the things right there on the tree item.
    /// </summary>
    public static class ExprEval
    {
        public static TreeItem Add(TreeItem left, TreeItem right)
        {
            if (left is Number leftNumber && right is Number rightNumber)
            {
                leftNumber.Value += rightNumber.Value;
                return leftNumber;
            }

            return new AdditiveLine(left, right);   
        }

        public static TreeItem Multiply(TreeItem left, TreeItem right)
        {
            var item = TryMultiplyNoNegate(left, right) ?? new TermLine(left, right);
            item.IsNegative = left.IsNegative ^ right.IsNegative;
            return item;
        }

        static TreeItem TryMultiplyNoNegate(TreeItem left, TreeItem right)
        {
            // Move any division or term-line to the left
            if (right is TermLine || right is Division)
            {
                var swap = left;
                left = right;
                right = swap;
            }

            switch (left)
            {
                // Number
                case Number number:
                    if (right is Number rightNumber)
                    {
                        number.Value *= rightNumber.Value;
                        return number;
                    }

                    return null;

                // Term line
                case TermLine line:
                    return MultiplyTermLineBy(line, right);

                // Division
                case Division division:
                    if (right is Division rightDivision)
                    {
                        division.Top = Multiply(division.Top, rightDivision.Top);
                        division.Bottom = Multiply(division.Bottom, rightDivision.Bottom);
                    }
                    else
                    {
                        division.Top = Multiply(division.Top, right);
                    }

                    return division;

                // Power
                case Power power:
                    if (right is Power rightPower)
                    {
                        // If the bases match, add the exponents together.
                        if (power.Base.Equals(rightPower.Base))
                            return new Power(power.Base, Add(power.Exponent, rightPower.Exponent));
                    }
                    else
                    {
                        // If there's no power, we'll count this as "b^1", so as long as "b" matches, we'll add 1.
                        if (power.Base.Equals(right))
                            return new Power(power.Base, Add(power.Exponent, new Number(1)));
                    }

                    return null;

                // Root
                case Root root:
                    if (right is Root rightRoot)
                        if (root.Index.Equals(rightRoot.Index))
                            return new Root(Multiply(root.Inner, rightRoot.Inner), root.Index);

                    return null;
                case Function func:

                    return null;
                default:
                    return null;
            }
        }

        static TreeItem MultiplyTermLineBy(TermLine line, TreeItem multiplyBy)
        {
            // Number
            if (multiplyBy is Number number)
            {
                line.Coefficient *= number.Value;
                return line;
            }

            // Another term-line
            else if (multiplyBy is TermLine rightLine)
            {
                line.Coefficient *= rightLine.Coefficient;

                // Multiply every part by the given terms.
                for (int i = 0; i < line.Terms.Count; i++)
                    line.Terms[i] = MultiplyTermLineByNonNumber(rightLine, line.Terms[i]);

                return line;
            }

            // Put anything else within the term line.
            else
                return MultiplyTermLineByNonNumber(line, multiplyBy);
        }

        static TreeItem MultiplyTermLineByNonNumber(TermLine line, TreeItem multiplyBy)
        {
            var againstType = multiplyBy.GetType();

            for (int i = 0; i < line.Terms.Count; i++)
            {
                // Find something that's the same as the "againstType".
                if (line.Terms[i].GetType() == againstType)
                {
                    // Now try to multiply them!
                    var multiplyAttempt = TryMultiplyNoNegate(line.Terms[i], multiplyBy);

                    // If we failed to multiply them, just put the "multiplyBy" into the term list.
                    if (multiplyAttempt == null) 
                        line.Terms.Add(multiplyBy);
                    else 
                        line.Terms[i] = multiplyAttempt;
                }
            }

            // If there wasn't anything to multiply by in there, add it in.
            line.Terms.Add(multiplyBy);
            return line;
        }

        public static TreeItem TryEvaluateFunction(Function function)
        {
            var arg = (Number)function.Arguments[0];

            switch (function.Type)
            {
                case Function.FunctionType.Sin:
                    
                    switch (arg.Value)
                    {
                    case 0:
                        return new Number(0);
                    case 30:
                        return new Division(new Number(1), new Number(2));
                    case 45:
                        return new Division(new Root(new Number(2)), new Number(2));
                    case 60:
                        return new Division(new Root(new Number(3)), new Number(2));
                    case 90:
                        return new Number(1);
                    }

                    break;
                case Function.FunctionType.Cos:

                    switch (arg.Value)
                    {
                        case 0:
                            return new Number(1);
                        case 30:
                            return new Division(new Number(3), new Number(2));
                        case 45:
                            return new Division(new Root(new Number(2)), new Number(2));
                        case 60:
                            return new Division(new Root(new Number(1)), new Number(2));
                        case 90:
                            return new Number(0);
                    }

                    break;
                case Function.FunctionType.Tan:

                    switch (arg.Value)
                    {
                        case 0:
                            return new Division(new Root(new Number(0)), new Number(2));
                        case 30:
                            return new Division(new Root(new Number(1)), new Root(new Number(3)));
                        case 45:
                            return new Division(new Root(new Number(2)), new Root(new Number(2)));
                        case 60:
                            return new Division(new Root(new Number(3)), new Number(1));
                        case 90:
                            return new Number(0);
                    }

                    break;
            }

            throw new NotImplementedException();
        }
    }
}
