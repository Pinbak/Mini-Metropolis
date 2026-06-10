using UnityEngine;

/// <summary>
///     A <see cref="Vector3"/> position compared to another <see cref="Vector3"/> position, with information about the
///     comparison
/// </summary>
public class RelativePosition
{
    public Vector3 Direction { get; }
    public Vector3 OppositeDirection { get; }
    public Vector3 Perpendicular { get; }
    public Vector3 OppositePerpendicular { get; }
    public float Angle { get; }
    public bool IsDiagonal { get; }
    public Vector3 A => _a;
    public Vector3 B => _b;
    
    private readonly Vector3 _a;
    private readonly Vector3 _b;

    public RelativePosition(Vector3 a, Vector3 b)
    {
        _a = a;
        _b = b;
        Direction = _b - _a;
        Direction.Normalize();
        OppositeDirection = Direction * -1;
        Perpendicular = Vector3.Cross(Vector3.up, Direction);
        OppositePerpendicular = Vector3.Cross(Vector3.up, OppositeDirection);
        Angle = Vector3.SignedAngle(a.normalized, Direction, Vector3.up);
        IsDiagonal = Direction.x != 0 && Direction.z != 0; // only works on flat terrain
    }
}