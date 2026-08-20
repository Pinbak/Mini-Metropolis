using System;

/// <summary>
///     The different nodes that can be found on the <see cref="Grid"/>.
/// </summary>
[Serializable]
public enum NodeType
{
    Empty,
    Road,
    Building,
    Parking
}