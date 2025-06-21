using System.Collections.Immutable;
using ClaspCompiler.IntermediateCil;
using ClaspCompiler.IntermediateCil.Abstract;

namespace ClaspCompiler.CompilerData
{
    internal sealed class ControlFlowGraph
    {
        internal readonly Dictionary<Label, HashSet<Label>> _edges;

        public ControlFlowGraph(IEnumerable<Label> labels)
        {
            _edges = labels.ToDictionary(x => x, x => new HashSet<Label>());
        }

        public void AddEdge(Label from, Label to)
        {
            _edges[from].Add(to);
        }

        public ImmutableHashSet<Label> GetEdges(Label from)
        {
            return [.. _edges[from]];
        }

        /// <summary>
        /// Reconstruct this graph, but with all edges pointing in the opposite direction.
        /// </summary>
        public ControlFlowGraph Transpose()
        {
            ControlFlowGraph output = new ControlFlowGraph(_edges.Keys);

            foreach (var vertex in _edges)
            {
                foreach (Label nextNode in vertex.Value)
                {
                    output.AddEdge(nextNode, vertex.Key);
                }
            }

            return output;
        }

        /// <summary>
        /// Return the set of nodes that have zero outgoing edges.
        /// </summary>
        public ImmutableHashSet<Label> GetTerminalNodes()
        {
            return _edges
                .Where(x => x.Value.Count == 0)
                .Select(x => x.Key)
                .ToImmutableHashSet();
        }

        public static ControlFlowGraph Uncover(IDictionary<Label, Block> labeledBlocks)
        {
            ControlFlowGraph cfg = new(labeledBlocks.Keys);

            foreach (var pair in labeledBlocks)
            {
                foreach (Instruction instr in pair.Value.Instructions)
                {
                    if (instr.Operator.IsJump()
                        && instr.Operand is Label jumpDest)
                    {
                        cfg.AddEdge(pair.Key, jumpDest);
                    }
                }
            }

            return cfg;
        }
    }
}
