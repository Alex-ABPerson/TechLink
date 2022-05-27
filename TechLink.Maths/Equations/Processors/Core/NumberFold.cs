﻿using System;
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
    /// </summary>
    internal class NumberFold : Processor
    {
        public override TreeItem Perform(TreeItem itm) => itm switch
        {
            AdditiveLine additive => PerformAdditive(additive),
            TermLine line => PerformTermLine(line),
            _ => throw new Exception()
        };

        public TreeItem PerformAdditive(AdditiveLine additive)
        {
            long? currentNum = null;
            for (int i = 0; i < additive.Items.Count; i++)
            {
                if (additive.Items[i] is Number num)
                {
                    currentNum ??= 0;
                    currentNum += num.Value;
                }
            }

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
            long? currentNum = null;
            for (int i = 0; i < line.Terms.Count; i++)
            {
                if (line.Terms[i] is Number num)
                {
                    currentNum ??= 1;
                    currentNum *= num.Value;
                }
            }

            // No numbers
            if (currentNum == null) return line;

            // Numbers make 0
            if (currentNum == 0) return new Number(0);

            // With numbers
            var newTreeItem = currentNum == 1 ? new TermLine() : new TermLine(new Number(currentNum.Value));

            for (int i = 0; i < line.Terms.Count; i++)
                if (line.Terms[i] is not Number)
                    newTreeItem.Terms.Add(line.Terms[i]);

            return newTreeItem;
        }
    }
}
