using System;
using System.Collections.Generic;
using System.Text;
using TechLink.Core;

namespace TechLink.Chemistry.Models
{
    public class ChemicalReaction
    {
        public ReactionPart[] Reactants;
        public ReactionPart[] Products;
    }

    public struct ReactionPart
    {
        public int MoleNo;
        public Particle Data;
    }

    public class Element : Particle
    {
        public int AtomicNumber;
        public int Number;
    }

    public class Molecule : Particle
    {
        public Element[] Elements;
    }

    public class Particle { }
}
