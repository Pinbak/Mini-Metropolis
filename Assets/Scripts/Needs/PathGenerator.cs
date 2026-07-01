using System.Collections.Generic;
using UnityEngine;

namespace Needs
{
    /// <summary>
    ///     Converts an A* found path of <see cref="Node"/>s into a <see cref="Vector3"/> path to be followed by a car
    /// </summary>
    public class PathGenerator
    {
        public List<Vector3> Path { get; private set; } = new();
        public bool PathGenerated { get; private set; }
        public Node CurrentNode { get; private set; }
        public Node NextNode { get; private set; }
        
        private readonly Pathfinding _pathfinding; // the actual a* algorithm to find a path from A to B
        private int _currentNodePointer;
        private int _currentPositionPointer;
        
        private readonly GridManager _gridManager;
        private readonly float _offset;
        private const float PathInset = .2f;
        private const float PathWidth = .4f; // todo get from elsewhere
        private const float PathStraightLength = .5f; // todo get from elsewhere
        private const float PathDiagonalLength = .7071f; // todo get from elsewhere
        private const float TurnSmoothness = .1f; // lower number is smoother

        public PathGenerator(GridManager gridManager, Node startPosition)
        {
            _gridManager = gridManager;
            _pathfinding = new Pathfinding(gridManager.Grid);
            CurrentNode = startPosition;
        }

        /// <summary>
        ///     Gets the next position in the path to visit
        /// </summary>
        public Vector3 GetNextPosition() // todo it's a bit of a mess
        {
            _currentPositionPointer++;
            if (_currentPositionPointer == Path.Count)
            {
                _currentPositionPointer = 0;
                CurrentNode = _pathfinding.Path[_currentNodePointer];
                _currentNodePointer++;
                if (_currentNodePointer == _pathfinding.Path.Length)
                {
                    _currentNodePointer = 0;
                    PathGenerated = false;
                    // we have reached our destination
                    return Vector3.zero;
                }
                NextNode = _pathfinding.Path[_currentNodePointer];
                GeneratePath();
            }
            return Path[_currentPositionPointer];
        }
        
        /// <summary>
        ///     After visiting this position, call <see cref="VisitedPosition"/>
        ///     Updates <see cref="PathGenerated"/> and <see cref="Path"/>
        /// </summary>
        private void VisitedPosition()
        {
            _currentPositionPointer++;
            if (_currentPositionPointer != Path.Count - 1) return;
            // if the car has reached the destination
            PathGenerated = false;
        }

        public void GeneratePath(Node start, Node goal) // todo remove start and use current node
        {
            _pathfinding.GeneratePath(start, goal);
            if (_pathfinding.ValidPathExists)
                GeneratePath();
        }
        
        private void GeneratePath()
        {
            if (_currentNodePointer == 0)
            {
                GenerateStartPath(_pathfinding.Path, _currentNodePointer);
                return;
            }
            if (_currentNodePointer == _pathfinding.Path.Length - 1)
            {
                GenerateEndPath(_pathfinding.Path, _currentNodePointer);
                return;
            }
            
            Path = new List<Vector3>();

            var position =
                _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[_currentNodePointer].X,
                    _pathfinding.Path[_currentNodePointer].Y));
            var nextPosition = _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[_currentNodePointer + 1].X,
                _pathfinding.Path[_currentNodePointer + 1].Y));
            var previousPosition = _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[_currentNodePointer - 1].X,
                _pathfinding.Path[_currentNodePointer - 1].Y));
                
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

            PathGenerated = true;
        }
        
        private void GenerateStartPath(Node[] nodePath, int currentNode)
        {
            Path = new List<Vector3>();
            var position =
                _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[_currentNodePointer].X,
                    _pathfinding.Path[_currentNodePointer].Y));
            Path.Add(position);
            PathGenerated = true;
        }

        private void GenerateEndPath(Node[] nodePath, int currentNode)
        {
            Path = new List<Vector3>();
            var position =
                _gridManager.GridToWorld(new Vector2Int(_pathfinding.Path[_currentNodePointer].X,
                    _pathfinding.Path[_currentNodePointer].Y));
            Path.Add(position);
            PathGenerated = true;
        }
    }
}