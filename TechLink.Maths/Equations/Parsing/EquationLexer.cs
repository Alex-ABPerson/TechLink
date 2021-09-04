using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core.Parsing;
using TechLink.Maths.Equations;

namespace TechLink.Maths.Equations.Parsing
{
    public class EquationLexer : Lexer<EquationToken>
    {
        public EquationLexer(string source) : base(source) { }

        public override void ProcessNext()
        {
            char ch = Read();

            switch (char.ToLower(ch))
            {
                case '+':
                    EmitToken(EquationTokenType.Add);
                    break;
                case '*':
                    EmitToken(EquationTokenType.Multiply);
                    break;
                case '/':
                    EmitToken(EquationTokenType.Division);
                    break;
                //case '%':
                //    EmitToken(EquationItemType.Modular);
                //    break;
                case '^':
                    EmitToken(EquationTokenType.PowerOf);
                    break;
                case '(':
                    EmitToken(EquationTokenType.OpeningBracket);
                    break;
                case ')':
                    EmitToken(EquationTokenType.ClosingBracket);
                    break;
                case '=':
                    EmitToken(EquationTokenType.Equals);
                    break;
                case ',':
                    EmitToken(EquationTokenType.Comma);
                    break;
                case '-':
                    EmitToken(EquationTokenType.Subtract);
                    break;
                case 's':
                    HandleS();
                    break;
                case 'c':
                    HandleC();
                    break;
                case 't':
                    HandleT();
                    break;
                default:

                    // Number
                    if (char.IsNumber(ch)) HandleInteger();

                    // Variable
                    else if (char.IsLetter(ch)) EmitVariable(ch);

                    break;
            }
        }

        void HandleInteger()
        {
            var newToken = new EquationToken(EquationTokenType.Number, CurrentPosition - 1);

            // Read the numerical characters.
            while (char.IsNumber(Peek())) CurrentPosition++;

            // Convert the number.
            newToken.NumericalData = uint.Parse(Source[newToken.Start..CurrentPosition]);

            Emit(newToken);
        }

        void HandleS()
        {
            // sqrt
            if (char.ToLower(Peek(0)) == 'q' && char.ToLower(Peek(1)) == 'r' && char.ToLower(Peek(2)) == 't' && char.ToLower(Peek(3)) == '(')
            {
                Consume(4);
                EmitToken(EquationTokenType.Root);
            }

            // sin
            else if (char.ToLower(Peek(0)) == 'i' && char.ToLower(Peek(1)) == 'n')
                HandleFunction(2, isInverse => EmitFunction(EquationTokenType.Sin, isInverse), NoFunction);

            else NoFunction();

            void NoFunction() => EmitVariable('s');
        }

        void HandleC()
        {
            // cos
            if (char.ToLower(Peek(0)) == 'o' && char.ToLower(Peek(1)) == 's')
                HandleFunction(2, isInverse => EmitFunction(EquationTokenType.Cos, isInverse), NoFunction);

            else NoFunction();

            void NoFunction() => EmitVariable('c');
        }

        void HandleT()
        {
            // tan
            if (char.ToLower(Peek(0)) == 'a' && char.ToLower(Peek(1)) == 'n')
                HandleFunction(2, isInverse => EmitFunction(EquationTokenType.Tan, isInverse), NoFunction);

            else NoFunction();

            void NoFunction() => EmitVariable('t');
        }

        void HandleFunction(int index, Action<bool> emitFunc, Action fail)
        {
            switch (Peek(index))
            {
                case '(':
                    Consume(index + 1);
                    emitFunc(false);

                    break;
                case '-':
                    if (Peek(index + 1) == '1' && Peek(index + 2) == '(')
                    {
                        Consume(index + 3);
                        emitFunc(true);
                    }
                    else fail();

                    break;
                default:
                    fail();
                    break;
            }

        }
        
        public void EmitToken(EquationTokenType type) => Emit(new EquationToken(type, CurrentPosition - 1));

        public void EmitFunction(EquationTokenType type, bool isInverse)
        {
            var token = new EquationToken(type, CurrentPosition - 1)
            {
                IsInverseFunction = isInverse
            };
            Emit(token);
        }

        public void EmitVariable(char v)
        {
            var token = new EquationToken(EquationTokenType.Variable, CurrentPosition - 1)
            {
                VariableType = v
            };
            Emit(token);
        }
    }
}
