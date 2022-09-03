using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations.Helpers
{
    public struct AdditiveCombination
    {
        public int CombinationSpread { get; }
        bool[] _combination;

        public AdditiveCombination(bool[] combination, int spread) => (_combination, CombinationSpread) = (combination, spread);
        public bool[] GetBoolArrayRepresentation() => _combination;
    }
}
