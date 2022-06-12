using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Processors.Core
{
    /// <summary>
    /// A processor responsible for transforming one-element lines into just that element
    /// </summary>
    internal class SingleItemLineExpander : Processor
    {
        public override TreeItem Perform(TreeItem itm) => itm switch
        {
            AdditiveLine line => PerformAdditive(line),
            TermLine term => PerformTerm(term),
            _ => throw new Exception("Incorrect use of RedundantLineExpander")
        };

        public TreeItem PerformAdditive(AdditiveLine line) => line.Items.Count == 1 ? line.Items[0] : line;
        public TreeItem PerformTerm(TermLine line) => line.Terms.Count == 1 ? line.Terms[0] : line;
    }
}
