using UnityEngine;

/// <summary>
///     A triangle container. Contains all the properties that define a triangles vertices. Used by <see cref=""/>
/// </summary>
public class Triangle
{
    public Vector3 A1 { get; }
    public Vector3 A2 { get; }
    public Vector3 A3 { get; }
    public Vector3 Centre { get; }

    public Triangle(Vector3 a1, Vector3 a2, Vector3 a3)
    {
        A1 = a1;
        A2 = a2;
        A3 = a3;
        Centre = (a1 + a2 + a3) / 3;
    }
}