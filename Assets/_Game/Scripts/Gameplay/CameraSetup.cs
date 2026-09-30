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

            float halfBoardWidth = (width * cellSize * 0.5f) + 0.4f;
            float halfBoardDepth = (height * cellSize * 0.5f) + 0.8f;
            float aspect = Mathf.Max(cam.aspect, 0.01f);
            cam.orthographicSize = Mathf.Max(halfBoardDepth, halfBoardWidth / aspect);
            
            // 55 degree isometric style looking down
            float heightOffset = 10f;
            float zOffset = -10f * Mathf.Tan((90f - 55f) * Mathf.Deg2Rad);
            
            transform.position = new Vector3(0, heightOffset, zOffset);
            transform.rotation = Quaternion.Euler(55f, 0, 0);
        }
    }
}
