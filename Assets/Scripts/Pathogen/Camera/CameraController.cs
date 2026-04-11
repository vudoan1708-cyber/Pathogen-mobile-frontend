using UnityEngine;
using UnityEngine.InputSystem;

namespace Pathogen
{
    public enum CameraMode
    {
        WideThirdPerson,
        TopDown,
        Isometric
    }

    [System.Serializable]
    public class CameraProfile
    {
        public CameraMode mode;
        public Vector3 offset;
        public Vector3 lookOffset;
        public float fov = 60f;
        public float followSpeed = 8f;
        public float rotationSpeed = 3f;
        public bool lockRotation = true;
    }

    public static class CameraFactory
    {
        public static CameraProfile Create(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.WideThirdPerson:
                    return new CameraProfile
                    {
                        mode = CameraMode.WideThirdPerson,
                        offset = new Vector3(0f, 12f, -14f),
                        lookOffset = new Vector3(0f, 0f, 5f),
                        fov = 55f,
                        followSpeed = 6f,
                        rotationSpeed = 2f,
                        lockRotation = true
                    };

                case CameraMode.TopDown:
                    return new CameraProfile
                    {
                        mode = CameraMode.TopDown,
                        offset = new Vector3(0f, 20f, -5f),
                        lookOffset = Vector3.zero,
                        fov = 50f,
                        followSpeed = 8f,
                        rotationSpeed = 0f,
                        lockRotation = true
                    };

                case CameraMode.Isometric:
                    return new CameraProfile
                    {
                        mode = CameraMode.Isometric,
                        offset = new Vector3(-10f, 15f, -10f),
                        lookOffset = new Vector3(2f, 0f, 2f),
                        fov = 45f,
                        followSpeed = 7f,
                        rotationSpeed = 0f,
                        lockRotation = true
                    };

                default:
                    return Create(CameraMode.WideThirdPerson);
            }
        }
    }

    public class CameraController : MonoBehaviour
    {
        public Transform target;
        public CameraMode currentMode = CameraMode.WideThirdPerson;

        [Header("Edge Pan (Desktop)")]
        public float edgePanSpeed = 20f;
        public float edgeThreshold = 5f; // Pixels from screen edge to start panning

        [Header("Recenter")]
        public KeyCode recenterKey = KeyCode.Space;

        public VirtualJoystick joystick;

        /// <summary>
        /// True when camera should follow champion: mobile device OR joystick is being used.
        /// </summary>
        public bool followChampion;

        private CameraProfile profile;
        private Camera cam;
        private Vector3 currentVelocity;

        private Vector3 focusPoint;
        private bool hasCentered;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
                cam = gameObject.AddComponent<Camera>();

            SetMode(currentMode);
        }

        void LateUpdate()
        {
            if (target == null || profile == null) return;

            // Lazy-find joystick on first use
            if (joystick == null)
                joystick = FindAnyObjectByType<VirtualJoystick>();

            bool joystickActive = joystick != null && joystick.IsDragging;
            followChampion = Application.isMobilePlatform || joystickActive;

            if (followChampion)
                UpdateFollowCamera();
            else
                UpdateDesktopCamera();
        }

        // ─── FOLLOW: joystick/mobile — camera tracks champion ───────────

        private void UpdateFollowCamera()
        {
            Vector3 desiredPosition = target.position + profile.offset;

            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPosition, ref currentVelocity,
                1f / profile.followSpeed);

            UpdateRotation(target.position);
        }

        // ─── DESKTOP: edge pan, champion-centered start ─────────────────

        private void UpdateDesktopCamera()
        {
            // First frame: center on champion
            if (!hasCentered)
            {
                focusPoint = target.position;
                hasCentered = true;
            }

            // Edge panning — move camera when mouse is at screen edges
            HandleEdgePan();

            // Recenter on champion with Space
            var kb = Keyboard.current;
            if (kb != null && kb.spaceKey.wasPressedThisFrame)
                focusPoint = target.position;

            // Camera follows the focus point (not the champion)
            Vector3 desiredPosition = focusPoint + profile.offset;

            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPosition, ref currentVelocity,
                1f / profile.followSpeed);

            UpdateRotation(focusPoint);
        }

        private void HandleEdgePan()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mousePos = mouse.position.ReadValue();
            Vector3 pan = Vector3.zero;

            float screenW = Screen.width;
            float screenH = Screen.height;

            // Left edge
            if (mousePos.x < edgeThreshold)
                pan.x -= 1f;
            // Right edge
            else if (mousePos.x > screenW - edgeThreshold)
                pan.x += 1f;

            // Bottom edge
            if (mousePos.y < edgeThreshold)
                pan.z -= 1f;
            // Top edge
            else if (mousePos.y > screenH - edgeThreshold)
                pan.z += 1f;

            if (pan.sqrMagnitude > 0.01f)
                focusPoint += pan.normalized * edgePanSpeed * Time.deltaTime;
        }

        // ─── SHARED ─────────────────────────────────────────────────────

        private void UpdateRotation(Vector3 lookTarget)
        {
            Vector3 lookAt = lookTarget + profile.lookOffset;
            Quaternion desiredRotation = Quaternion.LookRotation(lookAt - transform.position);

            if (profile.lockRotation)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, desiredRotation,
                    Time.deltaTime * profile.rotationSpeed * 2f);
            }
            else
            {
                transform.rotation = desiredRotation;
            }
        }

        public void SetMode(CameraMode mode)
        {
            currentMode = mode;
            profile = CameraFactory.Create(mode);

            if (cam != null)
                cam.fieldOfView = profile.fov;
        }

        public void CycleMode()
        {
            int next = ((int)currentMode + 1) % 3;
            SetMode((CameraMode)next);
        }

        public void SetCustomProfile(CameraProfile customProfile)
        {
            profile = customProfile;
            currentMode = customProfile.mode;
            if (cam != null)
                cam.fieldOfView = customProfile.fov;
        }

        /// <summary>
        /// Re-center camera on the champion.
        /// </summary>
        public void Recenter()
        {
            if (target != null)
                focusPoint = target.position;
        }
    }
}
