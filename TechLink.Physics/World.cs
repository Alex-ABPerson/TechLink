using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Physics
{
    public class World
    {
        public PhysicalObject[] Objects;

        public async void TickLoop()
        {
            while (true)
            {
                PerformTick();
                await Task.Delay(100);
            }
        }

        public void PerformTick()
        {
            // Move
        }
    }
}