using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core.Interfacing;
using TechLink.Core.Models;
using TechLink.Maths.Equations;
namespace TechLink.Maths.Equations.Renderers
{
    public static class ExpressionTextRenderer
    {
        public static void Render(TreeItem item) => RenderItem(item, false);

        public static void RenderItem(TreeItem item, bool withBrackets)
        {
            bool shouldWriteBrackets = withBrackets && IsBlock(item);
            if (shouldWriteBrackets) Interface.Write("(");

            switch (item)
            {
                case Number number:
                    Interface.Write(number.Value.ToString());
                    break;
                case Variable variable:
                    Interface.Write(variable.Name.ToString());
                    break;
                case Root root:
                    RenderRoot(root);
                    break;
                case Function function:
                    RenderFunction(function);
                    break;
                case Power power:
                    RenderItem(power.Base, true);
                    Interface.Write(" ^ ");
                    RenderItem(power.Exponent, true);
                    break;
                case TermLine term:
                    RenderTermLine(term);
                    break;
                case Division division:
                    RenderFraction(division);
                    break;
                case AdditiveLine op:
                    RenderAdditiveLine(op);
                    break;
            }

            if (shouldWriteBrackets) Interface.Write(")");
        }

        public static void RenderRoot(Root root)
        {
            Interface.Write("sqrt(");
            RenderItem(root.Inner, false);

            if (root.Index is not Number { Value: 2 })
            {
                Interface.Write(", ");
                RenderItem(root.Index, false);
            }

            Interface.Write(")");
        }

        public static void RenderFunction(Function function)
        {
            string functionTitle = function.Type switch
            {
                Function.FunctionType.Sin => "sin",
                Function.FunctionType.Cos => "cos",
                Function.FunctionType.Tan => "tan",
                _ => "unknown-f",
            };

            Interface.Write(functionTitle);

            if (function.IsInverse)
                Interface.Write("-1");

            Interface.Write("(");

            RenderItem(function.Arguments[0], false);

            for (int i = 1; i < function.Arguments.Count; i++)
            {
                Interface.Write(", ");
                RenderItem(function.Arguments[i], false);
            }

            Interface.Write(")");
        }

        public static void RenderAdditiveLine(AdditiveLine line)
        {
            if (line.Items.Count == 0) return;

            RenderItem(line.Items[0], true);

            for (int i = 1; i < line.Items.Count; i++)
            {
                Interface.Write(" + ");
                RenderItem(line.Items[i], true);
            }
        }

        public static void RenderTermLine(TermLine line)
        {
            // If there are no items, don't write anything.
            if (line.Terms.Count == 0) return;

            // If the term line is literally just "num x abc", just write "-abc".
            if (line.Terms.Count == 2)
            {
                bool isNegOnLeft = line.Terms[0] is Number;
                if (isNegOnLeft ^ line.Terms[1] is Number)
                {
                    Number numVal = isNegOnLeft ? (Number)line.Terms[0] : (Number)line.Terms[1];

                    // If it's -1, write "-" instead of "-1".
                    if (numVal.Value == -1)
                        Interface.Write("-");
                    else
                        RenderItem(numVal, false);

                    RenderItem(line.Terms[isNegOnLeft ? 1 : 0], true);
                    return;
                }
            }

            // Otherwise, render as normal
            RenderItem(line.Terms[0], true);

            for (int i = 1; i < line.Terms.Count; i++)
            {
                // Put brackets when we have two numbers after each other, or if we have a term line within the term line
                bool specialBrackets = line.Terms[i] is Number && line.Terms[i - 1] is Number || line.Terms[i] is TermLine;
                if (specialBrackets) Interface.Write("(");
                RenderItem(line.Terms[i], true);
                if (specialBrackets) Interface.Write(")");
            }
        }

        public static void RenderFraction(Division division)
        {
            RenderItem(division.Top, true);
            Interface.Write(" / ");
            RenderItem(division.Bottom, true);
        }

        static bool IsBlock(TreeItem item) => item is AdditiveLine;
    }
}
