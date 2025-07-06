using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.CompilerData
{
    internal sealed class DisjointTypeSet : IPrintable
    {
        private sealed class Node
        {
            public SchemeType Data { get; init; } // the actual type
            public int Rank { get; set; } // the number of children this node has
            private Node? _parent { get; set; } // the node this node is parented to, up to the set rep

            public bool IsRepresentative => _parent is null;

            public Node(SchemeType type)
            {
                Data = type;
                Rank = 0;
                _parent = null;
            }

            public Node GetParent() => _parent ?? this;
            public void SetParent(Node parent) => _parent = parent == this ? null : parent;

            public override string ToString() => Data.ToString();
        }

        private readonly Dictionary<SchemeType, Node> _nodes;
        private int _numSets;

        public DisjointTypeSet()
        {
            _nodes = [];
            _numSets = 0;
        }

        public int Count => _nodes.Count;

        public bool Contains(SchemeType type) => _nodes.ContainsKey(type);

        public void Add(SchemeType type)
        {
            FindNode(type);
        }

        /// <summary>
        /// Attempt to find the sets containing <paramref name="typeA"/> and <paramref name="typeB"/>
        /// and combine them into a single set.
        /// </summary>
        /// <remarks>New sets are implicitly created to hold the inputs, iff they don't already exist.</remarks>
        /// <returns>True if the union was successful, false if the types already shared a set.</returns>
        public bool Union(SchemeType typeA, SchemeType typeB)
        {
            Node parentA = FindNode(typeA).GetParent();
            Node parentB = FindNode(typeB).GetParent();

            if (parentA == parentB)
            {
                return false;
            }

            if (parentA.Rank >= parentB.Rank)
            {
                if (parentA.Rank == parentB.Rank)
                {
                    ++parentA.Rank;
                }

                parentB.SetParent(parentA);
            }
            else
            {
                parentA.SetParent(parentB);
            }

            --_numSets; // combining two sets together results in one set
            return true;
        }

        /// <summary>
        /// Try to find the representative of the set containing <paramref name="type"/>.
        /// If no sets contain <paramref name="type"/>, a new set will be created for it, and
        /// the new set's representative will be returned.
        /// </summary>
        public SchemeType Find(SchemeType type) => FindRep(FindNode(type)).Data;

        private static Node FindRep(Node node)
        {
            if (node.IsRepresentative)
            {
                return node;
            }
            else
            {
                Node rep = FindRep(node.GetParent());
                node.SetParent(rep);
                return rep;
            }
        }

        private Node FindNode(SchemeType type)
        {
            if (!_nodes.TryGetValue(type, out Node? node))
            {
                node = new(type);
                _nodes.Add(type, node);
                ++_numSets;
            }
            return node;
        }


        public bool BreaksLine => _nodes.Count > 0;
        public string AsString => $"Disjoint set with {Count} items in {_numSets} groups";
        public void Print(TextWriter writer, int indent)
        {
            writer.Write("(disjoint-set");
            indent += 2;
            writer.WriteLineIndent(indent);
            writer.WriteIndenting('(', ref indent);

            writer.WriteLineByLine(_nodes.Values, WriteNode, indent);

            writer.Write("))");
        }
        public sealed override string ToString() => AsString;

        private static void WriteNode(TextWriter writer, Node n, int indent)
        {
            writer.Write('[');
            writer.Write(n.Data);
            writer.Write(" → ");
            writer.Write(n.GetParent().Data);
            writer.Write(']');
        }
    }
}
