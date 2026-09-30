using UnityEngine;
using UnityEngine.InputSystem;

namespace ParcelEscape.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class PackageInputController : MonoBehaviour
    {
        private Camera _inputCamera;

        public void Initialize(Camera inputCamera)
        {
            _inputCamera = inputCamera;
        }

        private void Update()
        {
            if (_inputCamera == null || !TryGetPressPosition(out Vector2 screenPosition))
            {
                return;
            }

            Ray ray = _inputCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                return;
            }

            PackageView packageView = hit.collider.GetComponentInParent<PackageView>();
            packageView?.ForwardTap();
        }

        private static bool TryGetPressPosition(out Vector2 screenPosition)
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }
    }
}
