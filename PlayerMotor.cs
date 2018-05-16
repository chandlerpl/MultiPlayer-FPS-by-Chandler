using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// rotate player camera
namespace AxlPlay
{
    public class PlayerMotor : MonoBehaviour
    {

        [HideInInspector]
        public Camera cam;

        private Vector3 velocity = Vector3.zero;
        private Vector3 rotation = Vector3.zero;
        private float cameraRotationX = 0f;
        private float currentCameraRotationX = 0f;

        [SerializeField]
        private float cameraRotationLimit = 85f;

        private Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }
        public void Move(Vector3 _velocity)
        {
            velocity = _velocity;
        }
        void FixedUpdate()
        {
            PerformMovement();
            PerformRotation();
        }
        void PerformMovement()
        {
            if (velocity != Vector3.zero)
            {
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            }


        }
        //Perform rotation
        void PerformRotation()
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
            if (cam != null)
            {
                // Set our rotation and clamp it
                currentCameraRotationX -= cameraRotationX;
                currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

                //Apply our rotation to the transform of our camera
                cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
            }
        }
        // Gets a rotational vector
        public void Rotate(Vector3 _rotation)
        {
            rotation = _rotation;
        }
        // Gets a rotational vector for the camera
        public void RotateCamera(float _cameraRotationX)
        {
            cameraRotationX = _cameraRotationX;
        }

    }
}