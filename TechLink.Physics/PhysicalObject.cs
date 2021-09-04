using System;
using System.Collections.Generic;
using System.Text;

namespace TechLink.Physics
{
    public class PhysicalObject
    {
        public char Id { get; set; }
        public int Mass { get; set; }

        public int Acceleration { get; set; }
        public int Velocity { get; set; }

        public int Interia => Mass;
        public int Weight
        {
            get => Mass * 10;
        }
    }
}
