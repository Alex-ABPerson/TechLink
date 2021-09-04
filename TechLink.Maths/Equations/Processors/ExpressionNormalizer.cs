using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Processors
{
    // Processing techniques:
    //   
    /// <summary>
    /// Converts an expression to a "normalized" tree that can be processed much easier. See the details below.
    /// </summary>
    public static class ExpressionNormalizer
    {
        // NORMALIZED TREE:
        //
        // A normalized tree is a tree that follows a specific set of rules designed for making it easier to process. By the having
        // the tree always follow these rules, all the processors can process the tree a lot easier.
        //
        // Below the word "constant integer" will be a term used repeatedly, it simply means:
        //
        //   - Constant means the result does not depend on any variables that may only be known at processing-time.
        //   - Known functions such as "sin", "cos" and "tan" are also considered constant (this is due to rule #3).
        //
        // RULE #1:
        //   There should be NO powers with a CONSTANT INTEGER exponent. (Constant being an exact number with no variables)
        //   E.g. No "x^2", you should always have "xx".
        //   This allows situations such as "xxx / xx" to be really easily identified and actioned.
        //
        // RULE #2:
        //   There should be NO numbers present in term lines. Any numbers present should be evaluated and moved to the coefficient
        //   at normalization-time.
        //
        // RULE #3:
        //   There should be NO constant integer evaluations left unevaluated. So there should never be a "4 + 2", it will always just be "6".
        //   This applies to functions too which will be evaluated.
        public static void NormalizeExpression(TreeItem item)
        {
        }
    }
}
