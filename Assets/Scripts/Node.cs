using System.Collections.Generic;
using UnityEngine;

public class Node
{
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