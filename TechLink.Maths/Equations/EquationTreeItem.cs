using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TechLink.Core.Models;

namespace TechLink.Maths.Equations
{
    public abstract class TreeItem
    {
        public bool IsNegative = false;
        bool _hasHashCache = false;
        int _hashCache;

        public override bool Equals(object obj)
        {
            // If the hash codes don't match, then these definitely don't match.
            //if (GetHashCode() != obj.GetHashCode()) return false;

            // If the types don't match, they don't match.
            if (obj.GetType() != GetType()) return false;

            var asTreeItem = KnownCast<TreeItem>(obj);

            // If the negative sign doesn't match, they don't match.
            if (IsNegative != asTreeItem.IsNegative) return false;

            return Matches(asTreeItem);
        }

        public override int GetHashCode()
        {
            if (_hasHashCache)
                return _hashCache;

            _hashCache = ComputeHash();
            return base.GetHashCode();
        }

        protected abstract bool Matches(TreeItem right);

        // TODO: Hash codes for faster comparison times.
        protected virtual int ComputeHash() => 0;

        protected static T KnownCast<T>(object obj)
        {
#if DEBUG
            return (T)obj;
#else
            return Unsafe.As<T>(obj);
#endif
        }

        protected void InvalidateHashCode() => _hasHashCache = false;
    }

    public sealed class Number : TreeItem
    {
        public uint Value { get; set; }

        public Number(uint value) => Value = value;

        protected override bool Matches(TreeItem right)
        {
            var rightNumber = KnownCast<Number>(right);
            return Value == rightNumber.Value;
        }

        protected override int ComputeHash() => (int)Value;
    }

    public sealed class Variable : TreeItem
    {
        public char Name;

        public Variable(char name) => Name = name;

        protected override int ComputeHash() => Name;

        protected override bool Matches(TreeItem right)
        {
            var rightVariable = KnownCast<Variable>(right);
            return Name == rightVariable.Name;
        }
    }

    public sealed class AdditiveLine : TreeItem
    {
        public IList<TreeItem> Items = new List<TreeItem>();

        public AdditiveLine(TreeItem first) => Items.Add(first);
        public AdditiveLine(TreeItem first, TreeItem second)
        {
            Items.Add(first);
            Items.Add(second);
        }

        protected override bool Matches(TreeItem right)
        {
            var rightLine = KnownCast<AdditiveLine>(right);

            if (Items.Count != rightLine.Items.Count) return false;

            for (int i = 0; i < Items.Count; i++)
                if (!Items[i].Equals(rightLine.Items[i]))
                    return false;

            return true;
        }
    }

    public sealed class TermLine : TreeItem
    {
        public uint Coefficient { get; set; }
        public IList<TreeItem> Terms { get; set; } = new List<TreeItem>();

        public TermLine(TreeItem first) => Terms.Add(first);
        public TermLine(TreeItem first, TreeItem second)
        {
            Terms.Add(first);
            Terms.Add(second);
        }

        protected override bool Matches(TreeItem right)
        {
            var rightLine = KnownCast<TermLine>(right);

            if (Coefficient != rightLine.Coefficient) return false;
            if (Terms.Count != rightLine.Terms.Count) return false;

            for (int i = 0; i < Terms.Count; i++)
                if (!Terms[i].Equals(rightLine.Terms[i]))
                    return false;

            return true;
        }
    }

    public sealed class Division : TreeItem
    {
        public TreeItem Top;
        public TreeItem Bottom;

        public Division(TreeItem top, TreeItem bottom) => (Top, Bottom) = (top, bottom);

        protected override bool Matches(TreeItem right)
        {
            var rightDivision = KnownCast<Division>(right);
            return Top.Equals(rightDivision.Top) && Bottom.Equals(rightDivision.Bottom);
        }
    }

    public sealed class Root : TreeItem
    {
        public TreeItem Inner;
        public TreeItem Index;

        public Root(TreeItem inner) => (Inner, Index) = (inner, new Number(2));
        public Root(TreeItem inner, TreeItem index) => (Inner, Index) = (inner, index);

        protected override bool Matches(TreeItem right)
        {
            var rightRoot = KnownCast<Root>(right);
            return Inner.Equals(rightRoot.Inner) && Index.Equals(rightRoot.Index);
        }
    }

    public sealed class Power : TreeItem
    {
        public TreeItem Base;
        public TreeItem Exponent;

        public Power(TreeItem bas, TreeItem exponent) => (Base, Exponent) = (bas, exponent);

        protected override bool Matches(TreeItem right)
        {
            var rightPower = KnownCast<Power>(right);
            return Base.Equals(rightPower.Base) && Exponent.Equals(rightPower.Exponent);
        }
    }

    public sealed class Function : TreeItem
    {
        public enum FunctionType
        {
            Sin, Cos, Tan
        }

        public FunctionType Type;
        public bool IsInverse;
        public IList<TreeItem> Arguments = new List<TreeItem>();

        public Function(FunctionType type, TreeItem inner, bool isInverse)
        {
            (Type, IsInverse) = (type, isInverse);
            Arguments.Add(inner);
        }

        protected override bool Matches(TreeItem right)
        {
            var rightFunction = KnownCast<Function>(right);

            if (Type != rightFunction.Type || IsInverse != rightFunction.IsInverse || Arguments.Count != rightFunction.Arguments.Count) return false;

            for (int i = 0; i < Arguments.Count; i++)
                if (!Arguments[i].Equals(rightFunction.Arguments[i]))
                    return false;

            return true;
        }
    }
}
