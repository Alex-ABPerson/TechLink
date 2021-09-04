using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TechLink.Core.Parsing
{
    public abstract class Lexer<TToken>
    {
        protected int CurrentPosition;
        protected string Source;
        public List<TToken> Output = new();

        public Lexer(string currentString) => Source = currentString;

        public void Run()
        {
            while (CurrentPosition < Source.Length) ProcessNext();
        }

        public abstract void ProcessNext();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char Read()
        {
            if (CurrentPosition >= Source.Length) return '\0';
            return Source[CurrentPosition++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char Peek()
        {
            if (CurrentPosition >= Source.Length) return '\0';
            return Source[CurrentPosition];
        }

        public char Peek(int offset)
        {
            if (CurrentPosition + offset >= Source.Length) return '\0';
            return Source[CurrentPosition + offset];
        }

        public void Consume(int number) => CurrentPosition += number;

        public bool ReadIf(char c)
        {
            var next = Peek();

            if (next == c)
            {
                Read();
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Emit(TToken token) => Output.Add(token);
    }
}
