using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core.Models;

namespace TechLink.Maths.Equations
{
    public class Equation : TreeItem
    {
        public TreeItem Left;
        public TreeItem Right;

        public Equation(TreeItem left, TreeItem right) => (Left, Right) = (left, right);

        protected override bool Matches(TreeItem right)
        {
            var rightEquation = KnownCast<Equation>(right);
            return Left.Equals(rightEquation.Left) && Right.Equals(rightEquation.Right);
        }

        public override TreeItem Clone() => new Equation(Left.Clone(), Right.Clone());
    }
}