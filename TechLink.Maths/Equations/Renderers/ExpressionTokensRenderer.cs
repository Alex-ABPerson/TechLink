using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core.Interfacing;
using TechLink.Maths.Equations;
using TechLink.Maths.Equations.Parsing;

namespace TechLink.Maths.Equations.Renderers
{
    public static class ExpressionTokensRenderer
    {
        public static void Render(IEnumerable<EquationToken> tokens)
        {
            foreach (var token in tokens)
            {
                // Write type
                Interface.Write(token.Type.ToString());

                // Write extra information
                switch (token.Type)
                {
                    case EquationTokenType.Number:
                        Interface.Write("(");
                        Interface.Write(token.NumericalData.ToString());
                        Interface.Write(")");
                        break;
                    case EquationTokenType.Variable:
                        Interface.Write("(");
                        Interface.Write(token.VariableType.ToString());
                        Interface.Write(")");
                        break;
                    case EquationTokenType.Root:
                    case EquationTokenType.Sin:
                    case EquationTokenType.Cos:
                    case EquationTokenType.Tan:
                        if (token.IsInverseFunction)
                        {
                            Interface.Write("(");
                            Interface.Write("inverse");
                            Interface.Write(")");
                        }
                        
                        break;
                }

                // Write start
                Interface.SetColor(InterfaceColor.Unimportant);
                Interface.Write("[");
                Interface.Write(token.Start.ToString());
                Interface.Write("]");
                Interface.SetColor(InterfaceColor.Default);

                Interface.Write(" ");
            }

            Interface.WriteLine("\n");
        }
    }
}
