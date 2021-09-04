using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core;
using TechLink.Core.Interfacing;

namespace TechLink.Chemistry
{
    public class ChemistryFeatures : PageSet<object>
    {
        public override string Title => "Chemistry";
        public override IPage[] SubPages => throw new NotImplementedException();
        public override void Open(object state)
        {
            throw new NotImplementedException();
        }
    }
}
