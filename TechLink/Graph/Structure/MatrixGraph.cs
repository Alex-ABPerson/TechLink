using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechLink.Graph.Structure
{
    public class MatrixGraph<TNode, TEdge> : IGraph<TNode, TEdge>
    {
        readonly List<MatrixNode> _nodes;
        SquareList _edges;

        public MatrixGraph(int capacity = 4)
        {
            _nodes = new List<MatrixNode>();
            _edges = new SquareList(capacity);
        }

        public int Count => _edges.Count;

        public Node<TNode> Add(TNode node)
        {
            var newNode = new MatrixNode(node, _nodes.Count);
            _nodes.Add(newNode);
            _edges.GrowOne();
            return newNode;
        }

        public void Connect(Node<TNode> node1, Node<TNode> node2, TEdge edge)
        {
            
        }

        public IEnumerable<Node<TNode>> GetConnected(Node<TNode> node)
        {
            throw new NotImplementedException();
        }

        struct SquareList
        {
            int _length;
            TEdge[] _data;

            public int Count => _length;

            public SquareList(int capacity) => (_data, _length) = (new TEdge[capacity], 0);

            public void GrowOne()
            {
                // Increase our length
                _length++;

                // If this took us over capacity, grow the array.
                int potentialCapacity = _length * _length;
                if (potentialCapacity > _data.Length) Grow();
            }

            void Grow()
            {
                // Double in size
                int newLength = _length * 2;
                int newCapacity = newLength * newLength;

                Array.Resize(ref _data, newCapacity);
            }
            
            public TEdge this[int row, int column]
            {
                get => _data[row * _length + column];
                set => _data[row * _length + column] = value;
            }
        }

        class MatrixNode : Node<TNode>
        {
            public int Idx;

            public MatrixNode(TNode data, int idx) : base(data) => Idx = idx;
        }
    }
}
