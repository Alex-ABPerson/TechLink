using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Helpers
{
    public static class ExprMultiply
    {
        // TODO: Merge multiply and divide into one class? They follow similar code paths...
        public static TreeItem Multiply(TreeItem left, TreeItem right)
        {
            var item = TryMultiply(left, right) ?? new TermLine(left, right);
            return item;
        }

        static TreeItem? TryMultiply(TreeItem left, TreeItem right)
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
                            return new Power(power.Base, ExprAdditive.Add(power.Exponent, rightPower.Exponent));
                    }
                    else
                    {
                        // If there's no power, we'll count this as "b^1", so as long as "b" matches, we'll add 1.
                        if (power.Base.Equals(right))
                            return new Power(power.Base, ExprAdditive.Add(power.Exponent, new Number(1)));
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
            // Another term-line
            if (multiplyBy is TermLine rightLine)
            {
                // Multiply every part by the given terms.
                for (int i = 0; i < line.Terms.Count; i++)
                    line.Terms[i] = MultiplyTermLineByTerm(rightLine, line.Terms[i]);

                return line;
            }
            else
                return MultiplyTermLineByTerm(line, multiplyBy);
        }

        static TreeItem MultiplyTermLineByTerm(TermLine line, TreeItem multiplyBy)
        {
            var againstType = multiplyBy.GetType();

            for (int i = 0; i < line.Terms.Count; i++)
            {
                // Find something that's the same as the "againstType".
                if (line.Terms[i].GetType() == againstType)
                {
                    // Now try to multiply them!
                    var multiplyAttempt = TryMultiply(line.Terms[i], multiplyBy);

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
    }
}
