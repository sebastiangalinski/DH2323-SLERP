using UnityEngine;
using System.Collections;

namespace CompleteProject
{
    public enum CameraFollowMode
    {
        Stuck,   // Camera locked to tank position
        Smooth   // Camera smoothly follows tank
    }

    public class CameraFollow : MonoBehaviour
    {
        public Transform target;            // The position that that camera will be following.
        public CameraFollowMode followMode = CameraFollowMode.Smooth; // Camera following mode
        public float smoothing = 5f;        // The speed with which the camera will be following (in Smooth mode).

        // Look-ahead feature
        public bool enableLookAhead = true; // Toggle for look-ahead feature
        public float lookAheadDistance = 5f; // Distance to look ahead in direction of movement
        public float lookAheadSmoothness = 3f; // Smoothness of look-ahead transition

        // FOV zoom feature
        public bool enableFOVZoom = true;   // Toggle for FOV zoom feature
        public float baseFOV = 60f;         // Base FOV when stationary
        public float maxFOV = 75f;          // Maximum FOV when moving fast
        public float fovZoomSpeed = 10f;   // Speed at which to zoom FOV
        public float shootFOVBoost = 5f;   // FOV increase while shooting

        Vector3 offset;                     // The initial offset from the target.
        Rigidbody targetRigidbody;          // Reference to the target's rigidbody for velocity.
        Camera mainCamera;                  // Reference to the main camera.
        TankController tankController;      // Reference to the tank controller for movement velocity.
        ShellEmitter shellEmitter;          // Reference to the shell emitter for shooting state.
        Vector3 lookAheadOffset = Vector3.zero; // Current look-ahead offset
        float currentFOV;                   // Current FOV


        void Start ()
        {
            // Calculate the initial offset.
            offset = transform.position - target.position;

            // Get references to rigidbody, camera, tank controller, and shell emitter
            targetRigidbody = target.GetComponent<Rigidbody>();
            mainCamera = GetComponent<Camera>();
            tankController = target.GetComponent<TankController>();
            shellEmitter = target.GetComponentInChildren<ShellEmitter>();

            // Initialize FOV
            if (mainCamera != null)
            {
                currentFOV = baseFOV;
                mainCamera.fieldOfView = currentFOV;
            }
        }


        void FixedUpdate ()
        {
            // Get current movement velocity from tank controller
            float currentMovementVelocity = (tankController != null) ? tankController.m_CurrentMovementVelocity : 0f;
            float tankMaxSpeed = (tankController != null) ? tankController.m_Speed : 12f;

            // Calculate look-ahead offset based on movement velocity
            if (enableLookAhead && currentMovementVelocity > 0.5f)
            {
                // Scale look-ahead distance based on velocity (faster = look further ahead)
                float velocityRatio = Mathf.Clamp01(currentMovementVelocity / tankMaxSpeed);
                float scaledLookAheadDistance = Mathf.Lerp(lookAheadDistance * 0.3f, lookAheadDistance, velocityRatio);
                
                // Offset camera in the direction the tank body is pointing
                Vector3 targetLookAhead = target.forward * scaledLookAheadDistance;
                lookAheadOffset = Vector3.Lerp(lookAheadOffset, targetLookAhead, lookAheadSmoothness * Time.deltaTime);
            }
            else
            {
                // When stationary or moving backward, return to no offset
                lookAheadOffset = Vector3.Lerp(lookAheadOffset, Vector3.zero, lookAheadSmoothness * Time.deltaTime);
            }

            // Create a position the camera is aiming for based on the offset from the target.
            Vector3 targetCamPos = target.position + offset + lookAheadOffset;

            // Apply camera position based on follow mode
            if (followMode == CameraFollowMode.Stuck)
            {
                // Stuck mode: camera directly follows tank with no smoothing
                transform.position = targetCamPos;
            }
            else if (followMode == CameraFollowMode.Smooth)
            {
                // Smooth mode: smoothly interpolate between current and target position
                transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
            }

            // Update FOV based on movement velocity
            if (enableFOVZoom && mainCamera != null)
            {
                // Map movement velocity to FOV (0 speed = baseFOV, maxSpeed = maxFOV)
                float velocityRatio = Mathf.Clamp01(currentMovementVelocity / tankMaxSpeed);
                float targetFOV = Mathf.Lerp(baseFOV, maxFOV, velocityRatio);
                
                // Add FOV boost when holding left click to shoot (but not after shell is fired)
                if (Input.GetMouseButton(0) && (shellEmitter == null || !shellEmitter.m_Fired))
                {
                    // Calculate charge progress (0 to 1 over maxChargeTime)
                    float chargeForceRange = shellEmitter.m_MaxLaunchForce - shellEmitter.m_MinLaunchForce;
                    float chargeProgress = Mathf.Clamp01((shellEmitter.m_CurrentLaunchForce - shellEmitter.m_MinLaunchForce) / chargeForceRange);
                    
                    // Lerp the boost amount based on charge progress over maxChargeTime
                    float boostAmount = Mathf.Lerp(0f, shootFOVBoost, chargeProgress);
                    targetFOV += boostAmount;
                }
                
                currentFOV = Mathf.Lerp(currentFOV, targetFOV, fovZoomSpeed * Time.deltaTime);
                mainCamera.fieldOfView = currentFOV;
            }
        }
    }
}