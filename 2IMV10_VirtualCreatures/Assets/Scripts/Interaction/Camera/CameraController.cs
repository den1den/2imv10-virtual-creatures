﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera Controller")]
public class CameraController : MonoBehaviour {

    public enum CameraType { Free, Focus };

    public UnityEngine.Object virtualCreature;

    // Properties for camera motion
    public float flySpeed = 0.5f;
    public float accelerationRatio = 1;
    public float accelerationAmount = 3;
    public float slowDownRatio = 0.5f;
    public bool shift = false;
    public bool ctrl = false;

    // Properties for camera look at
    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    public Vector2 clampInDegrees = new Vector2(360, 180);
    public bool lockCursor;
    public Vector2 sensitivity = new Vector2(2, 2);
    public Vector2 smoothing = new Vector2(3, 3);
    public Vector2 targetDirection;

    void Start()
    {
        // Save own transform object
        _transform = this.transform;

        // Set target direction to the camera's initial orientation.
        targetDirection = transform.localRotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        CameraKeyboardControl();
        CameraLookAt();
    }


    public void CameraKeyboardControl()
    {
        // Detect if one of the Shift keys is pressed
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            shift = true;
            flySpeed *= accelerationRatio;
        }

        // Detect if one of the Shift keys is not pressed
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            shift = false;
            flySpeed /= accelerationRatio;
        }

        // Detect if one of the Ctrl keys is pressed
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            ctrl = true;
            flySpeed *= slowDownRatio;
        }

        // Detect if one of the Ctrl keys is not pressed
        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
        {
            ctrl = false;
            flySpeed /= slowDownRatio;
        }

        // 
        if (Input.GetAxis("Vertical") != 0)
        {
            transform.Translate(-transform.forward * flySpeed * Input.GetAxis("Vertical"));
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            transform.Translate(-transform.right * flySpeed * Input.GetAxis("Horizontal"));
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(transform.up * flySpeed * 0.5f);
        }

        else if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(-transform.up * flySpeed * 0.5f);
        }
    }


    public void CameraLookAt()
    {
        // Lock cursor
        Screen.lockCursor = lockCursor;

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
