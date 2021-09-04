using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core.Models;
using TechLink.Core.Parsing;

namespace TechLink.Maths.Equations.Parsing
{
    class EquationParser : ExpressionParser
    {
        protected override TreeItem Process(string input)
        {
            // Run the lexer.
            var lexer = new EquationLexer(input);
            lexer.Run();

            Source = lexer.Output;

            var left = Parse();

            if (IsPass(ReadIf((byte)EquationTokenType.Equals)))
            {
                var right = Parse();

                // Make sure there isn't another one.
                if (IsPass(ReadIf((byte)EquationTokenType.Equals))) 
                    throw new ParserErrorException("Equation can not have more than one value!");

                return new Equation(left, right);
            }

            return left;
        }
    }
}
