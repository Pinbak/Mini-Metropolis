using UnityEngine;

/// <summary>
///     A helper class for evaluating a quadratic Bézier curve.
/// </summary>
public static class BezierCurve
{
    public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        var point1 = Vector3.Lerp(a, b, t);
        var point2 = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(point1, point2, t);
    }
}