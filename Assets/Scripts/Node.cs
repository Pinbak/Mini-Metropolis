using System.Collections.Generic;

public class Node
{
    public List<Node> Neighbours { get; set; } = new();
    public NodeType Type { get; set; }

    public Node()
    {
    }
}