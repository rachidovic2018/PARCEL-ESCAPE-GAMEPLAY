using UnityEngine;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    public class BlockerView : MonoBehaviour
    {
        public void Initialize(GridPosition pos)
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.3f, 0.3f, 0.3f); // Dark gray
            }
        }
    }
}
