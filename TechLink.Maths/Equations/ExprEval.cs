using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Maths.Equations
{
    /// <summary>
    /// Performs operations on the exact tree item given.
    /// NOTE: None of these are recursive, meaning none of them touch the INNER items of the given trees, they only touch the things right there on the tree item.
    /// </summary>
    public static class ExprEval
    {
        

        public static TreeItem TryEvaluateFunction(Function function)
        {
            var arg = (Number)function.Arguments[0];

            switch (function.Type)
            {
                case Function.FunctionType.Sin:
                    
                    switch (arg.Value)
                    {
                    case 0:
                        return new Number(0);
                    case 30:
                        return new Division(new Number(1), new Number(2));
                    case 45:
                        return new Division(new Root(new Number(2)), new Number(2));
                    case 60:
                        return new Division(new Root(new Number(3)), new Number(2));
                    case 90:
                        return new Number(1);
                    }

                    break;
                case Function.FunctionType.Cos:

                    switch (arg.Value)
                    {
                        case 0:
                            return new Number(1);
                        case 30:
                            return new Division(new Number(3), new Number(2));
                        case 45:
                            return new Division(new Root(new Number(2)), new Number(2));
                        case 60:
                            return new Division(new Root(new Number(1)), new Number(2));
                        case 90:
                            return new Number(0);
                    }

                    break;
                case Function.FunctionType.Tan:

                    switch (arg.Value)
                    {
                        case 0:
                            return new Division(new Root(new Number(0)), new Number(2));
                        case 30:
                            return new Division(new Root(new Number(1)), new Root(new Number(3)));
                        case 45:
                            return new Division(new Root(new Number(2)), new Root(new Number(2)));
                        case 60:
                            return new Division(new Root(new Number(3)), new Number(1));
                        case 90:
                            return new Number(0);
                    }

                    break;
            }

            throw new NotImplementedException();
        }
    }
}
