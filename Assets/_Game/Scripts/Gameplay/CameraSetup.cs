using UnityEngine;

namespace ParcelEscape.Gameplay
{
    [RequireComponent(typeof(Camera))]
    public class CameraSetup : MonoBehaviour
    {
        public void SetupForBoard(int width, int height, float cellSize)
        {
            Camera cam = GetComponent<Camera>();
            cam.orthographic = true;
            
            float maxDimension = Mathf.Max(width, height) * cellSize;
            cam.orthographicSize = (maxDimension / 2f) + 2f; // padding + space
            
            // 55 degree isometric style looking down
            float heightOffset = 10f;
            float zOffset = -10f * Mathf.Tan((90f - 55f) * Mathf.Deg2Rad);
            
            transform.position = new Vector3(0, heightOffset, zOffset);
            transform.rotation = Quaternion.Euler(55f, 0, 0);
        }
    }
}
