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
    private IInteractive currentlyHovered;
    private Tool currentTool;
    private Dictionary<Tool, int> toolInteractionLayers;

    public enum Tool
    {
        AddEdge,
        Upgrade,
        AddLeaf
    }

    private void Start()
    {
        currentTool = Tool.AddEdge;
        toolInteractionLayers = new Dictionary<Tool, int>
        {
            {Tool.AddEdge, LayerMask.GetMask("Edges") },
            {Tool.Upgrade, LayerMask.GetMask("Edges") },
            {Tool.AddLeaf, LayerMask.GetMask("Nodes") }
        };
        playerInput = GetComponent<PlayerInput>();
        moveCamera = playerInput.actions.FindAction("MoveCamera", true);
        dragCamera = false;
        moveCameraDrag = playerInput.actions.FindAction("MoveCameraDrag", true);
        moveCameraDrag.performed += DetectMouseHover;
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
        select.started += InteractDependingOnTool;
        InputAction cancel = playerInput.actions.FindAction("Cancel", true);
        cancel.started += (args) =>
        {
            EdgeSpawner.Instance.StopNodePlacement();
        };
        currentlyHovered = null;
    }

    private void ChangeTool(Tool tool)
    {
        if (tool == currentTool)
        {
            return;
        }
        if (currentTool == Tool.AddEdge)
        {
            EdgeSpawner.Instance.StopNodePlacement();
        }
        currentTool = tool;
    }

    public void SelectAddEdgeTool()
    {
        ChangeTool(Tool.AddEdge);
    }

    public void SelectAddLeafTool()
    {
        ChangeTool(Tool.AddLeaf);
    }

    public void SelectUpgradeEdgeTool()
    {
        ChangeTool(Tool.Upgrade);
    }

    private void DetectMouseHover(InputAction.CallbackContext ctx)
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
        EdgeSpawner.Instance.OnSelectionMove(position);
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, toolInteractionLayers[Tool.AddEdge]);
        if (hit.transform != null)
        {
            IInteractive interactive = hit.transform.gameObject.GetComponent<IInteractive>();
            if (interactive != null)
            {
                currentlyHovered?.OnHoverEnd();
                currentlyHovered = interactive;
                currentlyHovered.OnHoverStart();
            }
            else
            {
                currentlyHovered?.OnHoverEnd();
                currentlyHovered = null;
            }
        }
        else
        {
            currentlyHovered?.OnHoverEnd();
            currentlyHovered = null;
        }
    }

    private void InteractDependingOnTool(InputAction.CallbackContext ctx)
    {
        int layerMask = toolInteractionLayers[currentTool];
        Vector2 start = Camera.main.ScreenToWorldPoint(moveCameraDrag.ReadValue<Vector2>());
        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.zero, Mathf.Infinity, layerMask);
        switch (currentTool)
        {
            case Tool.AddEdge:
                if (hit.transform == null)
                {
                    EdgeSpawner.Instance.PlaceNode();
                }
                else
                {
                    hit.transform.gameObject.GetComponent<IInteractive>()?.OnInteraction(start);
                }
                break;
            default:
                Debug.Log("Incorrect tool");
                break;
        }
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
