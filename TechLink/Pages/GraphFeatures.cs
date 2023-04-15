using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechLink.Core.Interfacing;

namespace TechLink
{
    public class GraphFeatures : PageSet
    {
        public override string Title => "Graph Structure";
        public override IPage[] SubPages => new IPage[]
        {

        };

        public override void Open()
        {
            while (true)
            {
                ShowMenu();
                Interface.Clear();
            }
        }
    }
}
