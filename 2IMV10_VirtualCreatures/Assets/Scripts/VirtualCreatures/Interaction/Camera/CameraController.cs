using UnityEngine;
using System.Collections;

namespace VirtualCreatures {
    [AddComponentMenu("Camera Controller")]
    public class CameraController : MonoBehaviour {

        public enum CameraMode { Free, Top, Focus };

        // Default camera mode
        public CameraMode mode = CameraMode.Free;


        // Properties for camera motion
        public float speed = 0.5f;
        public Vector3 translation;

        // Properties for camera look at
        Vector2 _mouseAbsolute;
        Vector2 _smoothMouse;

        public Vector2 clampInDegrees = new Vector2(360, 180);
        public bool lockCursor;
        public Vector2 sensitivity = new Vector2(2, 2);
        public Vector2 smoothing = new Vector2(3, 3);
        public Vector3 targetDirection;

        public Vector3 savedDirection;
        public Vector3 savedPosition;

        void Start()
        {
            // Set target direction to the camera's initial orientation.
            targetDirection = transform.localRotation.eulerAngles;
        }

        // Update is called once per frame
        void Update()
        {
            CameraKeyboardControl();
        }


        public void CameraKeyboardControl()
        {
            CameraModeFree();
            CameraModeTopView();

            if (transform.position.y < 10)
                transform.position = new Vector3(transform.position.x, 10, transform.position.z);

            if (transform.position.y > 1000)
                transform.position = new Vector3(transform.position.x, 1000, transform.position.z);
        }

        public void CameraModeFree()
        {
            if(mode == CameraMode.Free)
            {
                CameraLookAt();

                float orthogonalAxisValue = Input.GetAxis("Horizontal");
                float forwardAxisValue = Input.GetAxis("Vertical");

                transform.Translate(Vector3.forward * forwardAxisValue * Time.deltaTime * speed);
                transform.Translate(Vector3.right * orthogonalAxisValue * Time.deltaTime * speed);

            }
        }

        public void CameraModeTopView()
        {
            if(mode == CameraMode.Top)
            {
                float horizontalAxisValue = Input.GetAxis("Horizontal");
                float verticalAxisValue = Input.GetAxis("Vertical");
                float zoomAxisValue = Input.GetAxis("Mouse ScrollWheel");

                transform.Translate(Vector3.right * horizontalAxisValue * Time.deltaTime * speed * 10);
                transform.Translate(Vector3.up * verticalAxisValue * Time.deltaTime * speed * 10);
                transform.Translate(Vector3.forward * zoomAxisValue * Time.deltaTime * speed * 100);

                transform.LookAt(new Vector3(transform.position.x, -100, transform.position.z));
            }
        }


        public void changeMode(CameraMode mode)
        {
            if (mode == CameraMode.Top && this.mode != CameraMode.Top)
            {
                savedPosition = transform.position;
                savedDirection = targetDirection;

                transform.position = new Vector3(0, 200, 0);
            }
            else if (mode == CameraMode.Free && this.mode != CameraMode.Free)
            {
                targetDirection = savedDirection;

                transform.position = savedPosition;
            }

            this.mode = mode;
        }

        private void CameraLookAt()
        {
            // Lock cursor
            //creen.lockCursor = lockCursor;

            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler(targetDirection);

            // Get raw mouse input
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

            // Interpolate mouse movement over time
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            transform.localRotation = xRotation;

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            transform.localRotation *= targetOrientation;

            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;
        }
    }
}