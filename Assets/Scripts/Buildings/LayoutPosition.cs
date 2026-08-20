using UnityEngine;

namespace Buildings
{
    /// <summary>
    ///     A position in space. Used to define function for grid positions in building prefabs.
    /// </summary>
    public class LayoutPosition : MonoBehaviour
    {
        [field: SerializeField] public NodeType Type { get; set; }
    }
}