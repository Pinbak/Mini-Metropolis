using UnityEngine;

namespace Placement
{
    public interface IPlacementState
    {
        void MouseDown(Vector3 position);
        void MouseRelease();
        void MouseClick(Vector3 position);
        void KeyboardPress(KeyboardKeys key);
    }
}