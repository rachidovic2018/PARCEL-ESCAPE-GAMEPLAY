using System;
using System.Collections;
using UnityEngine;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    [RequireComponent(typeof(BoxCollider))]
    public class PackageView : MonoBehaviour
    {
        public int PackageId { get; private set; }
        public Action<int> OnTapped;

        public void Initialize(PackageState state)
        {
            PackageId = state.Id;
            SetColor(state.Color);
            CreateDirectionArrow(state.Direction);
        }

        private void SetColor(PackageColor color)
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer == null) return;
            
            renderer.material.color = color switch
            {
                PackageColor.Red => Color.red,
                PackageColor.Blue => Color.blue,
                PackageColor.Green => Color.green,
                PackageColor.Yellow => Color.yellow,
                PackageColor.Purple => new Color(0.5f, 0f, 0.5f),
                PackageColor.Orange => new Color(1f, 0.5f, 0f),
                _ => Color.white
            };
        }

        private void CreateDirectionArrow(PackageDirection direction)
        {
            GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.transform.SetParent(this.transform);
            
            // Destroy default collider on the arrow so it doesn't interfere
            Destroy(arrow.GetComponent<Collider>());
            
            // Simple arrow visual scaling and placement
            arrow.transform.localScale = new Vector3(0.2f, 0.2f, 0.5f);
            
            Vector3 offset = Vector3.zero;
            switch (direction)
            {
                case PackageDirection.Up: offset = new Vector3(0, 0, 0.25f); break;
                case PackageDirection.Down: offset = new Vector3(0, 0, -0.25f); break;
                case PackageDirection.Left: offset = new Vector3(-0.25f, 0, 0); break;
                case PackageDirection.Right: offset = new Vector3(0.25f, 0, 0); break;
            }
            
            arrow.transform.localPosition = offset + new Vector3(0, 0.5f, 0);
            
            var renderer = arrow.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.white;
            }
        }

        private void OnMouseDown()
        {
            OnTapped?.Invoke(PackageId);
        }

        public IEnumerator PlayEscapeAnimation(PackageDirection direction)
        {
            Vector3 startPos = transform.position;
            Vector3 delta = Vector3.zero;
            
            switch (direction)
            {
                case PackageDirection.Up: delta = new Vector3(0, 0, 5f); break;
                case PackageDirection.Down: delta = new Vector3(0, 0, -5f); break;
                case PackageDirection.Left: delta = new Vector3(-5f, 0, 0); break;
                case PackageDirection.Right: delta = new Vector3(5f, 0, 0); break;
            }

            Vector3 targetPos = startPos + delta;
            float elapsed = 0f;
            float duration = 0.5f;

            // Slight squash
            Vector3 originalScale = transform.localScale;
            transform.localScale = new Vector3(originalScale.x * 0.9f, originalScale.y * 1.1f, originalScale.z * 0.9f);

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
            Destroy(gameObject);
        }

        public IEnumerator PlayBlockedFeedback()
        {
            Vector3 originalPos = transform.position;
            float duration = 0.3f;
            float elapsed = 0f;
            float magnitude = 0.1f;

            while (elapsed < duration)
            {
                float xOffset = (Mathf.PingPong(elapsed * 10, magnitude) - magnitude / 2);
                float zOffset = (Mathf.PingPong(elapsed * 15, magnitude) - magnitude / 2);
                transform.position = originalPos + new Vector3(xOffset, 0, zOffset);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPos;
        }
    }
}
