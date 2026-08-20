using System.Collections.Generic;

/// <summary>
///     A node that exists on the <see cref="Grid"/>. Has two sorts of abstractions.
///     First, the positional information given as <see cref="X"/> and <see cref="Y"/>, second the graph information,
///     given as <see cref="Neighbours"/>.
/// </summary>
public class Node
{
    // The graph consists of this node and its recursive neighbours
    public List<Node> Neighbours { get; set; } = new();
    public NodeType Type { get; set; }
    
    public int X { get; set; }
    public int Y { get; set; }

    public Node(int x, int y)
    {
        X = x;
        Y = y;
    }
}