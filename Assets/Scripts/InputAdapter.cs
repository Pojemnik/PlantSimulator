using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputAdapter : MonoBehaviour
{
    [SerializeField]
    private CameraMovement cameraMovement;

    private PlayerInput playerInput;
    private InputAction moveCamera;
    private InputAction moveCameraDrag;
    private bool dragCamera;
    private Vector2 origin;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveCamera = playerInput.actions.FindAction("MoveCamera", true);
        dragCamera = false;
        moveCameraDrag = playerInput.actions.FindAction("MoveCameraDrag", true);
        InputAction moveCameraHold = playerInput.actions.FindAction("MoveCameraHold", true);
        moveCameraHold.started += (_) =>
        {
            origin = Camera.main.ScreenToWorldPoint(moveCameraDrag.ReadValue<Vector2>());
            dragCamera = true;
        };
        moveCameraHold.canceled += (_) => dragCamera = false;
        InputAction zoomCamera = playerInput.actions.FindAction("ZoomCamera", true);
        zoomCamera.started += (args) =>
        {
            float value = -args.ReadValue<float>();
            value = Mathf.Clamp(value, -1, 1);
            cameraMovement.ZoomCamera(value);
        };
    }

    private void LateUpdate()
    {
        cameraMovement.MoveCameraRelative(moveCamera.ReadValue<Vector2>() * Time.deltaTime);
        if (dragCamera)
        {
            Vector2 currentPos = Camera.main.ScreenToWorldPoint(moveCameraDrag.ReadValue<Vector2>());
            Vector2 diff = currentPos - origin;
            diff.x = -diff.x;
            diff.y = -diff.y;
            cameraMovement.MoveCameraRelative(diff, true);
        }
    }
}
