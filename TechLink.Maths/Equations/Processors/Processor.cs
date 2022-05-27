using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Processors
{
    public abstract class Processor
    {
        public abstract TreeItem Perform(TreeItem itm);
    }
}
