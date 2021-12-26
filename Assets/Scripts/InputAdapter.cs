using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputAdapter : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveCamera;
    private InputAction moveCameraDrag;
    private bool dragCamera;
    private Vector2 origin;
    private int interactionLayerMask;

    private void Start()
    {
        interactionLayerMask = LayerMask.GetMask("Interactive");
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
            CameraMovement.Instance.ZoomCamera(value);
        };
        InputAction select = playerInput.actions.FindAction("Select", true);
        select.started += (args) =>
        {
            Vector2 start = Camera.main.ScreenToWorldPoint(moveCameraDrag.ReadValue<Vector2>());
            Debug.Log(start);
            RaycastHit2D hit = Physics2D.Raycast(start, Vector2.zero, Mathf.Infinity, interactionLayerMask);
            if (hit.transform == null)
            {
                NodeSpawner.Instance.PlaceNode();
            }
            else
            {
                hit.transform.gameObject.GetComponent<IInteractive>()?.OnInteraction(start);
            }
        };
        InputAction cancel = playerInput.actions.FindAction("Cancel", true);
        cancel.started += (args) =>
        {
            NodeSpawner.Instance.StopNodePlacement();
        };
    }

    private void Update()
    {
        NodeSpawner.Instance.OnSelectionMove(Camera.main.ScreenToWorldPoint(moveCameraDrag.ReadValue<Vector2>()));
    }

    private void LateUpdate()
    {
        CameraMovement.Instance.MoveCameraRelative(moveCamera.ReadValue<Vector2>() * Time.deltaTime);
        if (dragCamera)
        {
            Vector2 currentPos = Camera.main.ScreenToWorldPoint(moveCameraDrag.ReadValue<Vector2>());
            Vector2 diff = currentPos - origin;
            diff.x = -diff.x;
            diff.y = -diff.y;
            CameraMovement.Instance.MoveCameraRelative(diff, true);
        }
    }
}
