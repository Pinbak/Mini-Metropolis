using Meshes;

namespace Junctions
{
    /// <summary>
    ///     The different types of junction. Used to classify nodes when generating a mesh in <see cref="NodeMesh"/>.
    /// </summary>
    public enum JunctionType
    {
        DeadEnd,
        Straight,
        AcuteCorner,
        RightAngleCorner,
        RightAngleDiagonalCorner,
        ObtuseCorner,
        ComplexAcuteCorner,
        ComplexCorner,
        Complex
    }
}