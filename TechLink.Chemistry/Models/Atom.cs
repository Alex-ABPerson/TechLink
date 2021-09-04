using System;
using System.Collections.Generic;
using System.Text;

namespace TechLink.Chemistry.Models
{
    public class Atom
    {
        public Nucleus Nucleus;
        public Electron[] Electrons;
    }

    public struct Nucleus
    {
        public Proton[] Protons;
        public Neutrons[] Neutrons;

        public int Mass => Protons.Length + Neutrons.Length;
    }

    public struct Proton { }
    public struct Neutrons { }
    public struct Electron { }
}
