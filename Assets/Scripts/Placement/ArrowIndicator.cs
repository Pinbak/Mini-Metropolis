using UnityEngine;

namespace Placement
{
    /// <summary>
    ///     An arrow which shows the player which way a building preview is facing. This is used to recolour it.
    /// </summary>
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