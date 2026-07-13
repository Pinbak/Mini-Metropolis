using UnityEngine;

namespace Placement
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ArrowIndicator : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        
        private void OnEnable()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void SetColour(Color colour)
        {
            _renderer.color = colour;
        }
    }
}