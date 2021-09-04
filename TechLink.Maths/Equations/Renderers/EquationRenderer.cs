using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechLink.Core.Interfacing;

namespace TechLink.Maths.Equations.Renderers
{
    public static class EquationRenderer
    {
        public static void Render(Equation equation)
        {
            ExpressionTextRenderer.Render(equation.Left);
            Interface.Write(" = ");
            ExpressionTextRenderer.Render(equation.Right);
        }
    }
}
