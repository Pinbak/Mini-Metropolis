using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    /// <summary>
    ///     Converts an A* found path of <see cref="Node"/>s into a <see cref="Vector3"/> path to be followed by an agent
    /// </summary>
    public class PathGenerator
    {
        public Node[] NodePath => _pathfinding.Path;
        
        public List<Vector3> Path { get; private set; } = new();
        public bool PathGenerated { get; private set; }
        
        private readonly Pathfinding _pathfinding; // the actual a* algorithm to find a path from A to B
        private readonly GameObject _agent; // the agent that has the position which is being used
        private readonly GridManager _gridManager;
        private readonly float _offset;
        private const float PathInset = .2f;
        private const float PathWidth = .4f; // todo get from elsewhere
        private const float PathStraightLength = .5f; // todo get from elsewhere
        private const float PathDiagonalLength = .7071f; // todo get from elsewhere
        private const float TurnSmoothness = .1f; // lower number is smoother

        public PathGenerator(GridManager gridManager, GameObject agent)
        {
            _gridManager = gridManager;
            _pathfinding = new Pathfinding(gridManager.Grid);
            _agent = agent;
        }
        
        public void GeneratePath(Node start, Node goal) // todo remove start and use current node
        {
            PathGenerated = false;
            _pathfinding.GeneratePath(start, goal);
            if (_pathfinding.ValidPathExists)
            {
                GenerateSteps(0);
                PathGenerated = true;
            }
        }
        
        public void GenerateSteps(int currentNode)
        {
            if (currentNode == 0)
            {
                GenerateStartPath(_pathfinding.Path, currentNode);
                return;
            }
            if (currentNode == _pathfinding.Path.Length - 1)
            {
                GenerateEndPath(_pathfinding.Path, currentNode);
                return;
            }
            
            Path = new List<Vector3>();

            var position =
                _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[currentNode].X,
                    _pathfinding.Path[currentNode].Y));
            var nextPosition = _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[currentNode + 1].X,
                _pathfinding.Path[currentNode + 1].Y));
            var previousPosition = _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[currentNode - 1].X,
                _pathfinding.Path[currentNode - 1].Y));
                
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
                
            nextPoint = directionToNextPosition * (nextRoadLength) + nextPoint;
            previousPoint = directionToPreviousPosition * (previousRoadLength) + previousPoint;
            var movedPosition = position + nextPerpendicular * ((PathWidth - PathInset)  * .5f); // the centre of the node that is shifted to the correct lane
            
            // for straight roads, we don't need to Bezier
            if (directionToNextPosition == directionToPreviousPosition * -1)
                Path.Add(movedPosition);
            else
            {
                for (var t = TurnSmoothness; t < 1f; t += TurnSmoothness)
                {
                    var point = BezierCurve.EvaluateQuadratic(previousPoint, movedPosition, nextPoint, t);
                    Path.Add(point);
                }

            }

        }
        
        // todo these two
        private void GenerateStartPath(Node[] nodePath, int currentNode)
        {
            Path = new List<Vector3>();
            var position =
                _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[currentNode].X,
                    _pathfinding.Path[currentNode].Y));
            var nextPosition = _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[currentNode + 1].X,
                _pathfinding.Path[currentNode + 1].Y));
            Vector3 directionToNextPosition = nextPosition - position;
            directionToNextPosition.Normalize();
            var nextPerpendicular = Vector3.Cross(Vector3.up, directionToNextPosition);
            var movedPosition = position + nextPerpendicular * ((PathWidth - PathInset)  * .5f); // the centre of the node that is shifted to the correct lane
            Path.Add(movedPosition);
        }

        private void GenerateEndPath(Node[] nodePath, int currentNode)
        {
            Path = new List<Vector3>();
            var position =
                _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[currentNode].X,
                    _pathfinding.Path[currentNode].Y));
            Path.Add(position);
        }
    }
}