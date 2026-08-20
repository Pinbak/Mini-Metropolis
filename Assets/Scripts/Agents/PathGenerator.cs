using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Agents
{
    /// <summary>
    ///     Converts an A* found path of <see cref="Node"/>s into a <see cref="Vector3"/> path to be followed by an agent
    /// </summary>
    public class PathGenerator
    {
        public Node[] NodePath { get; private set; }
        
        public List<Vector3> Path { get; private set; } = new();
        public bool PathGenerated { get; private set; }
        
        private readonly Pathfinding _pathfinding; // the actual a* algorithm to find a path from A to B
        private readonly PathMover _agent; // the agent that has the position which is being used
        private readonly GridManager _gridManager;
        private readonly float _offset;
        private const float PathInset = .2f;
        private const float PathWidth = .4f;
        private const float PathStraightLength = .5f;
        private const float PathDiagonalLength = .7071f;
        private const float TurnSmoothness = .1f; // lower number is smoother

        public PathGenerator(GridManager gridManager, PathMover agent)
        {
            _gridManager = gridManager;
            _pathfinding = new Pathfinding();
            _agent = agent;
        }
        
        /// <summary>
        ///     Generate a path from <see cref="start"/> to <see cref="goal"/>.
        /// </summary>
        public void GeneratePath(Node modifiedStart, Node modifiedEnd, Node start, Node goal)
        {
            PathGenerated = false;
            _pathfinding.GeneratePath(start, goal);
            var nodePath = _pathfinding.Path.ToList();
            // insert the first and last positions, which are the parking spaces at each end of the journey
            nodePath.Insert(0, modifiedStart);
            nodePath.Add(modifiedEnd);
            NodePath = nodePath.ToArray();
            
            // generate just the first steps of the path which the path mover will follow along
            if (_pathfinding.ValidPathExists)
            {
                GenerateSteps(0);
                PathGenerated = true;
            }
        }
        
        /// <summary>
        ///     Generate steps given a current node from the <see cref="NodePath"/>. Generates next immediate steps, rather
        ///     than the whole path in one go.
        /// </summary>
        public void GenerateSteps(int currentNode)
        {
            // beginning
            if (currentNode == 0)
            {
                GenerateStartPath();
                return;
            }
            // end
            if (currentNode == NodePath.Length - 1)
            {
                GenerateEndPath();
                return;
            }
            
            // if not beginning or end, take the next and last nodes and Bezier between them to create a smooth path
            Path = new List<Vector3>();

            Vector3 position =
                _gridManager.GridToWorld(new Vector2Int(NodePath[currentNode].X,
                    NodePath[currentNode].Y));
            Vector3 nextPosition = _gridManager.GridToWorld(new Vector2Int(NodePath[currentNode + 1].X,
                NodePath[currentNode + 1].Y));
            Vector3 previousPosition = _gridManager.GridToWorld(new Vector2Int(NodePath[currentNode - 1].X,
                NodePath[currentNode - 1].Y));
            if (currentNode == NodePath.Length - 2)
                nextPosition = GetEndNode(); // penultimate

            // get the next road position
            Vector3 directionToNextPosition = nextPosition - position;
            directionToNextPosition.Normalize();
            var nextPerpendicular = Vector3.Cross(Vector3.up, directionToNextPosition);
            var nextPoint = position + nextPerpendicular * ((PathWidth - PathInset)  * .5f);
            var nextIsDiagonal = directionToNextPosition.x != 0 && directionToNextPosition.z != 0;
            var nextRoadLength = nextIsDiagonal ? PathDiagonalLength : PathStraightLength;

            // same as above but for the previous position
            Vector3 directionToPreviousPosition = previousPosition - position;
            directionToPreviousPosition.Normalize();
            var previousPerpendicular = Vector3.Cross(Vector3.up, directionToPreviousPosition);
            var previousPoint = position - previousPerpendicular * ((PathWidth - PathInset) * .5f); // using negative as is opposite direction
            var previousIsDiagonal = directionToPreviousPosition.x != 0 && directionToPreviousPosition.z != 0;
            var previousRoadLength = previousIsDiagonal ? PathDiagonalLength : PathStraightLength;
            
            nextPoint = directionToNextPosition * (nextRoadLength) + nextPoint;
            previousPoint = directionToPreviousPosition * (previousRoadLength) + previousPoint;
            var movedPosition = position + nextPerpendicular * ((PathWidth - PathInset)  * .5f); // the centre of the node that is shifted to the correct lane
            
            // for straight roads, don't need to Bezier
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
        
        private void GenerateStartPath()
        {
            Path = new List<Vector3> { GetStartNode() };
        }

        private Vector3 GetStartNode() => _agent.WorldPosition;

        private void GenerateEndPath()
        {
            Path = new List<Vector3> { GetEndNode() };
        }

        private Vector3 GetEndNode() => _agent.Destination.transform.position;
    }
}