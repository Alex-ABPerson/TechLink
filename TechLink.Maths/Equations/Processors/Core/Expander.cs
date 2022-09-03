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

            // Multiply everything in this line by our value
            for (int i = 0; i < line.Terms.Count; i++)
                if (i != additiveIdx)
                {
                    TreeItem currentlyMultiplyingBy = line.Terms[i];

                    for (int j = 0; j < newAdditiveLine.Items.Count; j++)
                    {
                        // If it's a term line, just insert into that line, just to save us unnecessary work later.
                        if (newAdditiveLine.Items[j] is TermLine innerLine)
                            innerLine.Terms.Add(currentlyMultiplyingBy);
                        else
                            newAdditiveLine.Items[j] = new TermLine(newAdditiveLine.Items[j], currentlyMultiplyingBy);
                    }
                }

            return newAdditiveLine;
        }
    }
}
