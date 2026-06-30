using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    public class Vector3Path
    {
        // Converts a path of nodes into a viable path of vector3 points to follow
        public List<Vector3> Path { get; private set; } = new();

        private readonly GridManager _gridManager;
        private readonly float _offset;
        private const float PathHeight = .2f;
        private const float PathInset = .2f;
        private const float PathWidth = .4f; // todo get from elsewhere
        private const float PathStraightLength = .5f; // todo get from elsewhere
        private const float PathDiagonalLength = .7071f; // todo get from elsewhere

        public Vector3Path(GridManager gridManager)
        {
            _gridManager = gridManager;
        }

        public void GeneratePath(List<Node> nodePath)
        {
            Path = new List<Vector3>();

            for (var i = 1; i < nodePath.Count - 1; i++)
            {
                var position = _gridManager.GridToWorld(new Vector2Int(nodePath[i].X, nodePath[i].Y));
                var nextPosition = _gridManager.GridToWorld(new Vector2Int(nodePath[i + 1].X, nodePath[i + 1].Y));
                var previousPosition = _gridManager.GridToWorld(new Vector2Int(nodePath[i - 1].X, nodePath[i - 1].Y));
                
                // todo add the ability to swap road sides
                Vector3 directionToNextPosition = nextPosition - position;
                directionToNextPosition.Normalize();
                var nextPerpendicular = Vector3.Cross(Vector3.up, directionToNextPosition);
                var nextPoint = position + nextPerpendicular * ((PathWidth - PathInset)  * .5f);
                var nextIsDiagonal = directionToNextPosition.x != 0 && directionToNextPosition.z != 0;
                var nextRoadLength = nextIsDiagonal ? PathDiagonalLength : PathStraightLength;

                // todo just repeated from above
                Vector3 directionToPreviousPosition = previousPosition - position;
                directionToPreviousPosition.Normalize();
                var previousPerpendicular = Vector3.Cross(Vector3.up, directionToPreviousPosition);
                var previousPoint = position - previousPerpendicular * ((PathWidth - PathInset) * .5f); // using negative as is opposite direction
                var previousIsDiagonal = directionToPreviousPosition.x != 0 && directionToPreviousPosition.z != 0;
                var previousRoadLength = previousIsDiagonal ? PathDiagonalLength : PathStraightLength;
                
                nextPoint = directionToNextPosition * (nextRoadLength * .5f) + nextPoint;
                previousPoint = directionToPreviousPosition * (previousRoadLength * .5f) + previousPoint;
                
                Path.Add(new Vector3(nextPoint.x, PathHeight, nextPoint.z));
                Path.Add(new Vector3(previousPoint.x, PathHeight, previousPoint.z));
                
            }
            
        }
        
    }
}