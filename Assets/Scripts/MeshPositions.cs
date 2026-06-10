using UnityEngine;

/// <summary>
///     A <see cref="RelativePosition"/> with added points for mesh generation
/// </summary>
public class MeshPositions : RelativePosition
{
    public Vector3 PerpendicularLeft { get; }
    public Vector3 OppositePerpendicularLeft { get; }
    public Vector3 PerpendicularRight { get; }
    public Vector3 OppositePerpendicularRight { get; }
    public Vector3 ForwardLeft { get; }
    public Vector3 ForwardRight { get; }
    
    private readonly float _meshWidth;
    private readonly float _meshDiagonalLength;
    private readonly float _meshStraightLength;
    
    public MeshPositions(Vector3 a, Vector3 b, float meshWidth, float unitSize = 1) : base(a, b)
    {
        _meshWidth = meshWidth;
        var halfMeshWidth = meshWidth * .5f;
        _meshStraightLength = unitSize * .5f;
        _meshDiagonalLength = Mathf.Sqrt(unitSize * unitSize + unitSize * unitSize);
        var length = IsDiagonal ? _meshDiagonalLength : _meshStraightLength;
        
        PerpendicularLeft = a - Perpendicular * halfMeshWidth;
        OppositePerpendicularLeft = a - OppositePerpendicular * halfMeshWidth;
        PerpendicularRight = a + Perpendicular * halfMeshWidth;
        OppositePerpendicularRight = a + OppositePerpendicular * halfMeshWidth;
        ForwardLeft = Direction * length + PerpendicularLeft;
        ForwardRight = Direction * length + PerpendicularRight;
    }
}