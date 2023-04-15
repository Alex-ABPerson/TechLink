using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Graph.Structure
{
    public class Node<T> 
    {
        public int Index;
        public T Data;

        public Node(T data) => Data = data;
    }
}
