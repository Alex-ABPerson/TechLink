using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TechLink.Core.Interfacing;
using TechLink.Core.Models;

namespace TechLink.Core.Parsing
{
    public abstract class Parser<TToken, TOutput> where TToken : IToken where TOutput : class
    {
        public IList<TToken> Source;
        protected int CurrentPosition;

        public bool HasRemaining => CurrentPosition < Source.Count;

        public TOutput Run(string source)
        {
            try
            {
                return Process(source);
            }
            catch (ParserErrorException ex)
            {
                Interface.WriteLine(ex.Message, InterfaceColor.Error);
                Interface.ReadLine();
                return null;
            }
        }

        protected abstract TOutput Process(string source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ParseReadResult Read()
        {
            if (!HasRemaining) return ParseReadResult.ForEOT();
            return ParseReadResult.ForPass(CurrentPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ParseReadResult Peek()
        {
            if (!HasRemaining) return ParseReadResult.ForEOT();
            return ParseReadResult.ForPass(CurrentPosition);
        }

        protected ParseReadResult ReadIf(byte type)
        {
            if (!HasRemaining) return ParseReadResult.ForEOT();

            var nextTokenType = Source[CurrentPosition].GetTokenType();
            if (nextTokenType != type) return ParseReadResult.ForNotFound(CurrentPosition); 
            
            return ParseReadResult.ForPass(CurrentPosition++);
        }

        protected ParseReadResult ReadIf(byte[] types)
        {
            if (!HasRemaining) return ParseReadResult.ForEOT();

            if (PeekMatches(types)) return ParseReadResult.ForPass(CurrentPosition++);
            else return ParseReadResult.ForNotFound(CurrentPosition);
        }

        protected bool PeekMatches(byte[] types)
        {
            if (!HasRemaining) return false;

            var nextTokenType = Source[CurrentPosition].GetTokenType();
            for (int i = 0; i < types.Length; i++)
                if (nextTokenType == types[i])
                    return true;

            return false;
        }

        protected bool ReadIfInline(byte match, out ParseReadResult res)
        {
            res = ReadIf(match);
            return res.ResultType == ParserReadResultCode.Pass;
        }

        protected bool ReadIfInline(byte[] match, out ParseReadResult res)
        {
            res = ReadIf(match);
            return res.ResultType == ParserReadResultCode.Pass;
        }

        protected enum ParserReadResultCode { Pass, NotFound, EOT }
        protected struct ParseReadResult
        {
            public ParserReadResultCode ResultType;
            public int TokenPos;

            public static ParseReadResult ForPass(int tokenPos)
            {
                return new ParseReadResult()
                {
                    ResultType = ParserReadResultCode.Pass,
                    TokenPos = tokenPos
                };
            }

            public static ParseReadResult ForNotFound(int tokenPos)
            {
                return new ParseReadResult()
                {
                    TokenPos = tokenPos,
                    ResultType = ParserReadResultCode.NotFound
                };
            }

            public static ParseReadResult ForEOT()
            {
                return new ParseReadResult()
                {
                    ResultType = ParserReadResultCode.EOT
                };
            }
        }

        protected bool IsPass(ParseReadResult res) => res.ResultType == ParserReadResultCode.Pass;

        protected ParseReadResult ExpectPass(ParseReadResult res)
        {
            return res.ResultType switch
            {
                ParserReadResultCode.Pass => res,
                ParserReadResultCode.NotFound => throw new ParserErrorException($"Unexpected token '{Source[res.TokenPos]}' at position '{Source[res.TokenPos].GetStart()}'"),
                ParserReadResultCode.EOT => throw new ParserErrorException($"Unexpected end-of-text."),
                _ => throw new Exception("Uh, something went very wrong..."),
            };
        }

        protected TToken GetToken(ParseReadResult res)
        {
            if (res.ResultType != ParserReadResultCode.Pass) throw new Exception("An attempt was made to access a token on a failed parse result.");
            return Source[res.TokenPos];
        }

        protected byte GetTokenType(ParseReadResult res)
        {
            if (res.ResultType != ParserReadResultCode.Pass) throw new Exception("An attempt was made to access a token on a failed parse result.");
            return Source[res.TokenPos].GetTokenType();
        }        
    }
}
