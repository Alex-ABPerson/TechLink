using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Graph.Structure
{
    public interface IGraph<TNode, TEdge>
    {
        Node<TNode> Add(TNode node);
        int Count { get; }
        void Connect(Node<TNode> node1, Node<TNode> node2, TEdge edge);
        IEnumerable<Node<TNode>> GetConnected(Node<TNode> node);
    }
}
