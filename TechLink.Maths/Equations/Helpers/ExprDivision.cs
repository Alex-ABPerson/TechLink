using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Helpers
{
    public enum SpecialDivisionResult
    {
        None,
        PerfectDivision, // a.k.a a/a
        UnchangedValue, // a.k.a a/1
    }

    public static class ExprDivision
    {
        // TODO: Merge multiply and divide into one class? They follow similar code paths...

        public static TreeItem Divide(TreeItem left, TreeItem right)
        {
            var item = TryDivide(left, right) ?? new Division(left, right);
            return item;
        }

        // Tries to the divide the item given. Returns null it's just "a/b".
        static TreeItem? TryDivide(TreeItem left, TreeItem right) => TryDivide(left, right, out TreeItem? res) switch
        {
            SpecialDivisionResult.PerfectDivision => new Number(1),
            SpecialDivisionResult.UnchangedValue => left,
            SpecialDivisionResult.None => res,
            _ => throw new Exception()
        };

        static SpecialDivisionResult TryDivide(TreeItem left, TreeItem right, out TreeItem? res)
        {
            res = null;

            if (left == right) return SpecialDivisionResult.PerfectDivision;
            if (right is Number { Value: 1 }) return SpecialDivisionResult.UnchangedValue;
            if (right is Number { Value: 0 }) throw new Exception("Undefined value in process");

            res = TryDivideNonSpecial(left, right);
            return SpecialDivisionResult.None;
        }

        static TreeItem? TryDivideNonSpecial(TreeItem left, TreeItem right) 
        {
            // Ensure if there is a division or term-lines that it is definitely on the left.
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
                        number.Value /= rightNumber.Value;
                        return number;
                    }

                    return null;

                // Term line
                case TermLine line:
                    return DivideTermLineBy(line, right);

                // Division
                case Division division:
                    if (right is Division rightDivision)
                    {
                        division.Top = ExprMultiply.Multiply(division.Top, rightDivision.Bottom);
                        division.Bottom = ExprMultiply.Multiply(division.Bottom, rightDivision.Top);
                    }
                    else
                    {
                        division.Bottom = ExprMultiply.Multiply(division.Bottom, right);
                    }

                    return division;

                // Power
                case Power power:
                    if (right is Power rightPower)
                    {
                        // If the bases match, add the exponents together.
                        if (power.Base.Equals(rightPower.Base))
                            return new Power(power.Base, ExprAdditive.Subtract(power.Exponent, rightPower.Exponent));
                    }
                    else
                    {
                        // If there's no power, we'll count this as "b^1", so as long as "b" matches, we'll add 1.
                        if (power.Base.Equals(right))
                            return new Power(power.Base, ExprAdditive.Subtract(power.Exponent, new Number(1)));
                    }

                    return null;

                // Root
                case Root root:
                    if (right is Root rightRoot)
                        if (root.Index.Equals(rightRoot.Index))
                            return new Root(ExprMultiply.Multiply(root.Inner, rightRoot.Inner), root.Index);

                    return null;
                case Function func:

                    return null;
                default:
                    return null;
            }
        }

        static TreeItem DivideTermLineBy(TermLine line, TreeItem divideBy)
        {
            // Another term-line
            if (divideBy is TermLine rightLine)
            {
                // Divide every part by the given terms.
                for (int i = 0; i < line.Terms.Count; i++)
                    line.Terms[i] = DivideTermLineByTerm(rightLine, line.Terms[i]);

                return line;
            }
            else
                return DivideTermLineByTerm(line, divideBy);
        }

        static TreeItem DivideTermLineByTerm(TermLine line, TreeItem divideBy)
        {
            var againstType = divideBy.GetType();

            for (int i = 0; i < line.Terms.Count; i++)
            {
                // Find something that's the same as the "againstType".
                if (line.Terms[i].GetType() == againstType)
                {
                    // Now try to divide it!
                    var divideAttempt = TryDivide(line.Terms[i], divideBy, out TreeItem? res);

                    // If it didn't change, we'll skip this
                    if (divideAttempt == SpecialDivisionResult.None) continue;

                    // If it was a perfect division, remove the item entirely.
                    if (divideAttempt == SpecialDivisionResult.PerfectDivision)
                    {
                        line.Terms.RemoveAt(i--);
                        continue;
                    }

                    // Otherwise, put the value we got in there.
                }
            }

            // If there wasn't anything to multiply by in there, add it in.
            line.Terms.Add(divideBy);
            return line;
        }
    }
}
