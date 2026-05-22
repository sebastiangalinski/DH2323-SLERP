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

        Vector3 offset;                     // The initial offset from the target.
        Rigidbody targetRigidbody;          // Reference to the target's rigidbody for velocity.
        Camera mainCamera;                  // Reference to the main camera.
        Vector3 lookAheadOffset = Vector3.zero; // Current look-ahead offset
        float currentFOV;                   // Current FOV


        void Start ()
        {
            // Calculate the initial offset.
            offset = transform.position - target.position;

            // Get references to rigidbody and camera
            targetRigidbody = target.GetComponent<Rigidbody>();
            mainCamera = GetComponent<Camera>();

            // Initialize FOV
            if (mainCamera != null)
            {
                currentFOV = baseFOV;
                mainCamera.fieldOfView = currentFOV;
            }
        }


        void FixedUpdate ()
        {
            // Calculate forward velocity (velocity in the direction the player is facing)
            Vector3 forwardVelocity = Vector3.zero;
            float forwardSpeed = 0f;
            
            if (targetRigidbody != null)
            {
                forwardVelocity = Vector3.Project(targetRigidbody.velocity, target.forward);
                forwardSpeed = forwardVelocity.magnitude;
            }

            // Calculate look-ahead offset only when tank is moving FORWARD
            if (enableLookAhead && forwardSpeed > 0.5f)
            {
                // Offset camera in the direction the tank body is pointing
                Vector3 targetLookAhead = target.forward * lookAheadDistance;
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

            // Update FOV based on forward velocity
            if (enableFOVZoom && mainCamera != null)
            {
                // Get tank's max speed from TankController
                TankController tankController = target.GetComponent<TankController>();
                float tankMaxSpeed = (tankController != null) ? tankController.m_Speed : 12f;

                // Map forward speed to FOV (0 speed = baseFOV, maxSpeed = maxFOV)
                float speedRatio = Mathf.Clamp01(forwardSpeed / tankMaxSpeed);
                float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedRatio);
                currentFOV = Mathf.Lerp(currentFOV, targetFOV, fovZoomSpeed * Time.deltaTime);
                mainCamera.fieldOfView = currentFOV;
            }
        }
    }
}