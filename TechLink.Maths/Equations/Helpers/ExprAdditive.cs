using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Helpers
{
    public static class ExprAdditive
    {
        public static TreeItem Add(TreeItem left, TreeItem right)
        {
            if (left is Number leftNumber && right is Number rightNumber)
            {
                leftNumber.Value += rightNumber.Value;
                return leftNumber;
            }

            return new AdditiveLine(left, right);
        }

        public static TreeItem Subtract(TreeItem left, TreeItem right)
        {
            if (left is Number leftNumber && right is Number rightNumber)
            {
                leftNumber.Value -= rightNumber.Value;
                return leftNumber;
            }

            return new AdditiveLine(left, ExprMultiply.Multiply(new Number(-1), right));
        }
    }
}
