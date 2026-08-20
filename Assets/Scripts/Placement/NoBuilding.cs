using UnityEngine;

namespace Placement
{
    /// <summary>
    ///     If the player is not placing anything.
    /// </summary>
    public class NoBuilding : IPlacementState
    {
        public void EnterState() { }

        public void ExitState() { }

        public void MouseDown(Vector3 position) { }

        public void MouseRelease() { }

        public void MouseClick(Vector3 position) { }

        public void KeyboardPress(KeyboardKeys key) { }

        public void MouseMove(Vector3 position) { }
    }
}