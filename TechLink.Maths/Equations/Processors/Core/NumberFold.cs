using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Processors.Core
{
    /// <summary>
    /// Folds constant numbers in additive/term lines.
    /// Examples:
    /// 2 + 4 to 6 or 
    /// 4 * 8 to 32 or 
    /// 2 + a + 6 = 8 + a
    /// 4 / 2 to 2
    /// </summary>
    internal class NumberFold : Processor
    {
        public override bool Required => true;

        public override TreeItem Perform(TreeItem itm) => itm switch
        {
            AdditiveLine additive => PerformAdditive(additive),
            TermLine line => PerformTermLine(line),
            Division div => PerformDivision(div),
            _ => throw new Exception()
        };

        public TreeItem PerformAdditive(AdditiveLine additive)
        {
            bool hadMultipleNumbers = false;
            long? currentNum = null;
            for (int i = 0; i < additive.Items.Count; i++)
            {
                if (additive.Items[i] is Number num)
                {
                    hadMultipleNumbers = currentNum != null;
                    currentNum ??= 0;
                    currentNum += num.Value;
                }
            }

            // If there weren't multiple numbers in there, don't make any changes.
            // This is just to save us from doing "2x + 3" into "3 + 2x" which looks really silly.
            if (!hadMultipleNumbers) return additive;

            if (currentNum != null)
            {
                var newTreeItem = currentNum == 0 ? new AdditiveLine() : new AdditiveLine(new Number(currentNum.Value));

                for (int i = 0; i < additive.Items.Count; i++)
                    if (additive.Items[i] is not Number)
                        newTreeItem.Items.Add(additive.Items[i]);

                return newTreeItem;
            }

            return additive;
        }

        public TreeItem PerformTermLine(TermLine line)
        {
            bool hasNonNumbers = false;

            long? currentNum = null;
            for (int i = 0; i < line.Terms.Count; i++)
            {
                if (line.Terms[i] is Number num)
                {
                    currentNum ??= 1;
                    currentNum *= num.Value;
                }
                else hasNonNumbers = true;
            }

            // No numbers
            if (currentNum == null) return line;

            // Numbers make 0
            if (currentNum == 0) return new Number(0);

            // No non-number components (saves perf and extra path steps)
            if (!hasNonNumbers) return new Number(currentNum.Value);

            // With numbers
            var newTreeItem = currentNum == 1 ? new TermLine() : new TermLine(new Number(currentNum.Value));

            for (int i = 0; i < line.Terms.Count; i++)
                if (line.Terms[i] is not Number)
                    newTreeItem.Terms.Add(line.Terms[i]);

            return newTreeItem;
        }

        public TreeItem PerformDivision(Division div)
        {
            if (div.Top is not Number topNum || div.Bottom is not Number bottomNum) return div;

            // Only do this if they're divisible.
            if (topNum.Value % bottomNum.Value != 0) return div;

            return new Number(topNum.Value / bottomNum.Value);
        }
    }
}
