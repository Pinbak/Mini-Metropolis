using UnityEngine;

namespace Placement
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LineDrawer : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private const float Width = .4f;
        private Quaternion _defaultRotation;

        private void OnEnable()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void ShowRenderer() => _renderer.enabled = true;
        public void HideRenderer() => _renderer.enabled = false;

        public void ResetLength() => _renderer.size = new Vector2(Width, Width);

        public void DrawLine(Vector3 a, Vector3 b)
        {
            var direction = b - a;
            transform.position = (a + b) * .5f;
            transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
            
            var angles = transform.eulerAngles;
            angles.x = Mathf.Round(angles.x / 45f) * 45f;
            angles.y = Mathf.Round(angles.y / 45f) * 45f;
            angles.z = Mathf.Round(angles.z / 45f) * 45f;
            transform.eulerAngles = angles;
            
            _renderer.size = new Vector2(Width, direction.magnitude);
        }

        public void SetColour(Color colour) =>_renderer.color = colour;
    }
}