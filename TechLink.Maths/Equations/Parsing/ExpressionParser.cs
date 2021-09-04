using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core.Models;
using TechLink.Core.Parsing;
using TechLink.Maths.Equations.Parsing;

// expression   = additive
// additive     = division ( ( "+" | "-" ) division)*
// division     = term ( "/" term )*
// term         = index index* | index " * " index
// power        = negate ( "^" negate)*
// negate       = "-"* root
// root         = function | "sqrt(" expression ")"
// function     = primary | ("sin" | "cos" | "tan") "-1"? "(" expression ")"
// primary      = NUMBER | VARIABLE | "(" expression ")"
namespace TechLink.Maths.Equations.Parsing
{
    public class ExpressionParser : Parser<EquationToken, TreeItem>
    {
        static readonly byte[] ADDITIVE_TOKENS = new byte[] { (byte)EquationTokenType.Add, (byte)EquationTokenType.Subtract };
        static readonly byte[] FUNCTION_TOKENS = new byte[] { (byte)EquationTokenType.Sin, (byte)EquationTokenType.Cos, (byte)EquationTokenType.Tan };
        static readonly byte[] PRIMARY_TOKENS = new byte[] { (byte)EquationTokenType.Number, (byte)EquationTokenType.Variable, (byte)EquationTokenType.OpeningBracket };
        static readonly byte[] FUNCTION_AND_PRIMARY_TOKENS = new byte[] { (byte)EquationTokenType.Number, (byte)EquationTokenType.Variable, (byte)EquationTokenType.OpeningBracket, (byte)EquationTokenType.Root, (byte)EquationTokenType.Sin, (byte)EquationTokenType.Cos, (byte)EquationTokenType.Tan };

        public ExpressionParser() { }

        protected override TreeItem Process(string source)
        {
            // Run the lexer.
            var lexer = new EquationLexer(source);
            lexer.Run();

            Source = lexer.Output;
            return Parse();
        }

        protected TreeItem Parse() => ParseAdditive();

        TreeItem ParseAdditive()
        {
            var currentItem = ParseDivision();

            // Handle any "+"s and "-"s
            if (ReadIfInline(ADDITIVE_TOKENS, out var additiveSign))
            {
                var newLine = new AdditiveLine(currentItem);

                do
                {
                    var itm = ParseDivision();

                    // Negate it if our addition was a subtract.
                    if ((EquationTokenType)GetTokenType(additiveSign) == EquationTokenType.Subtract) 
                        itm.IsNegative = !itm.IsNegative;

                    newLine.Items.Add(itm);
                }
                while (ReadIfInline(ADDITIVE_TOKENS, out additiveSign));

                return newLine;
            }

            return currentItem;
        }

        TreeItem ParseDivision()
        {
            var currentItem = ParseTerm();

            // Handle any "/"s
            while (IsPass(ReadIf((byte)EquationTokenType.Division)))
            {
                var rightSide = ParseTerm();
                currentItem = new Division(currentItem, rightSide);
            }

            return currentItem;
        }

        TreeItem ParseTerm()
        {
            var currentItem = ParsePowerOf();

            // Handle anything that could make up a term
            if (PeekMatches(FUNCTION_AND_PRIMARY_TOKENS) || IsPass(ReadIf((byte)EquationTokenType.Multiply)))
            {
                var newLine = new TermLine(currentItem);

                do
                    newLine.Terms.Add(ParsePowerOf());
                while (PeekMatches(FUNCTION_AND_PRIMARY_TOKENS) || IsPass(ReadIf((byte)EquationTokenType.Multiply)));

                return newLine;
            }

            return currentItem;
        }

        TreeItem ParsePowerOf()
        {
            var currentItem = ParseNegate();

            while (IsPass(ReadIf((byte)EquationTokenType.PowerOf)))
            {
                var rightPart = ParseNegate();
                currentItem = new Power(currentItem, rightPart);
            }

            return currentItem;
        }

        TreeItem ParseNegate()
        {
            bool isNegate = false;

            while (IsPass(ReadIf((byte)EquationTokenType.Subtract)))
                isNegate = !isNegate;

            TreeItem root = ParseRoot();
            root.IsNegative = isNegate;
            return root;
        }

        TreeItem ParseRoot()
        {
            if (IsPass(ReadIf((byte)EquationTokenType.Root)))
            {
                var firstParam = Parse();
                var secondParam = IsPass(ReadIf((byte)EquationTokenType.Comma)) ? Parse() : new Number(2);

                ReadClosingBracket();

                return new Root(firstParam, secondParam);
            }

            return ParseFunction();
        }

        TreeItem ParseFunction()
        {
            if (ReadIfInline(FUNCTION_TOKENS, out ParseReadResult res))
            {
                var inner = Parse();
                ReadClosingBracket();

                var functionType = (EquationTokenType)GetTokenType(res) switch
                {
                    EquationTokenType.Sin => Function.FunctionType.Sin,
                    EquationTokenType.Cos => Function.FunctionType.Cos,
                    EquationTokenType.Tan => Function.FunctionType.Tan,
                    _ => throw new Exception()
                };

                return new Function(functionType, inner, GetToken(res).IsInverseFunction);
            }

            return ParsePrimary();
        }

        TreeItem ParsePrimary()
        {
            var primaryType = ExpectPass(ReadIf(PRIMARY_TOKENS));

            var token = GetToken(primaryType);
            switch (token.Type)
            {
                case EquationTokenType.Number:
                    return new Number(token.NumericalData);
                case EquationTokenType.Variable:
                    return new Variable(token.VariableType);
                case EquationTokenType.OpeningBracket:
                    var expression = Parse();
                    ReadClosingBracket();

                    return expression;
                default:
                    throw new Exception();
            }
        }

        void ReadClosingBracket() => ExpectPass(ReadIf((byte)EquationTokenType.ClosingBracket));
    }
}
