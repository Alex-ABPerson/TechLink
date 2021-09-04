using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TechLink.Core.Parsing;

namespace TechLink.Maths.Equations.Parsing
{
    public enum EquationTokenType : byte
    {
        None,

        // Primitives
        Number, Variable,

        // Operators - Subtraction is treated as "+ -"
        Add, Subtract, Multiply, Division, PowerOf,

        // Root
        Root,

        // Functions
        Sin, Cos, Tan,

        // Tokens only
        Comma, OpeningBracket, ClosingBracket, Equals
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct EquationToken : IToken
    {
        [FieldOffset(0)]
        public EquationTokenType Type;

        [FieldOffset(4)]
        public int Start;

        [FieldOffset(8)]
        public uint NumericalData;

        [FieldOffset(8)]
        public char VariableType;

        [FieldOffset(8)]
        public bool IsInverseFunction;

        public EquationToken(EquationTokenType type, int start) => (Type, Start, NumericalData, VariableType, IsInverseFunction) = (type, start, 0, '\0', false);

        public override string ToString()
        {
            return Type.ToString();
        }

        public byte GetTokenType() => (byte)Type;
        public int GetStart() => Start;
    }
}
