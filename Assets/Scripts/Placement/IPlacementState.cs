using UnityEngine;

namespace Placement
{
    public interface IPlacementState
    {
        void EnterState();
        void ExitState();
        void MouseDown(Vector3 position);
        void MouseRelease();
        void MouseClick(Vector3 position);
        void KeyboardPress(KeyboardKeys key);
        void MouseMove(Vector3 position);
    }
}