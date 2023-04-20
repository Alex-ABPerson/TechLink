#define FEATURE_EXPAND_DOUBLE_ADDITIVES // This feature enables special code paths to improve the expansion of "(a+b)(c+d)" to go directly to the unsimplified result instead of doing "a(c+d) + b(c+d)" first 
                                        // - it saves us a lot of processing paths!
                                        // This is an enableable feature because disabling it is really useful for testing out the core and how it copes with avoiding unnecessary paths and such

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Processors.Core
{
    /// <summary>
    /// Expands "any(val + val + ...)".
    /// </summary>
    internal class Expander : Processor
    {
        public override string Title => "Expansion";

        // TODO: Consider adding polynomialXpolynomial support for perf (less paths to process)
        public override TreeItem Perform(TreeItem itm)
        {
            TermLine line = (TermLine)itm;

            // If there's only one item, nothing to expand
            if (line.Terms.Count == 1) return line;

            // Find an additive line, then expand everything into it.
            for (int i = 0; i < line.Terms.Count; i++)
                if (line.Terms[i] is AdditiveLine additive)
                    return MultiplyAdditiveByEverythingElse(line, additive, i);

            return line;
        }

        private static AdditiveLine MultiplyAdditiveByEverythingElse(TermLine line, AdditiveLine curr, int additiveIdx)
        {
            // Create a new line to store the expanded version
            AdditiveLine newAdditiveLine = (AdditiveLine)curr.Clone();

            // Go through all the stuff on the outside and multiply each thing in our line by it
            for (int i = 0; i < line.Terms.Count; i++)
                if (i != additiveIdx)
                {
                    TreeItem currentMultiplier = line.Terms[i];

#if FEATURE_EXPAND_DOUBLE_ADDITIVES
                    if (currentMultiplier is AdditiveLine currentMultiplierAdditive && currentMultiplierAdditive.Items.Count > 1)
                    {
                        // We're going to need a new additive line, and do "newAdditiveLine * currentMultiplierAdditive" into it.
                        AdditiveLine additiveExpandedLine = new AdditiveLine(new List<TreeItem>(newAdditiveLine.Items.Count * currentMultiplierAdditive.Items.Count));

                        // Cross-multiply everything
                        for (int j = 0; j < newAdditiveLine.Items.Count; j++)
                            for (int k = 0; k < currentMultiplierAdditive.Items.Count; k++)
                                additiveExpandedLine.Items.Add(new TermLine(newAdditiveLine.Items[j], currentMultiplierAdditive.Items[k]));

                        newAdditiveLine = additiveExpandedLine;
                    }
                    else
#endif
                        MultiplyEachBy(currentMultiplier);

                    void MultiplyEachBy(TreeItem by)
                    {
                        for (int j = 0; j < newAdditiveLine.Items.Count; j++)
                        {
                            // If it's a term line, just insert into that line, to save us unnecessary work later.
                            if (newAdditiveLine.Items[j] is TermLine innerLine)
                                innerLine.Terms.Add(currentMultiplier);
                            else
                            {
                                newAdditiveLine.Items[j] = new TermLine(newAdditiveLine.Items[j], currentMultiplier);
                            }
                        }
                    }
                }

            return newAdditiveLine;
        }
    }
}
