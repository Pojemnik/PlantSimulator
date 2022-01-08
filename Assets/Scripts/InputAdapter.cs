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
    }

    private void ChangeTool(Tool tool)
    {
        EdgeSpawner.Instance.StopNodePlacement();
        EdgeUpgrader.Instance.HideTemporaryEdge();
        if (tool == currentTool)
        {
            return;
        }
        currentTool = tool;
        Debug.Log(currentTool);
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
        switch (currentTool)
        {
            case Tool.AddEdge:
                EdgeSpawner.Instance.OnSelectionMove(position);
                break;
            case Tool.Upgrade:
                RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, toolInteractionLayers[Tool.AddEdge]);
                PlantEdge edge = hit.transform?.gameObject.GetComponent<PlantEdge>();
                if (edge != null)
                {
                    EdgeUpgrader.Instance.TestUpgrade(edge);
                }
                else
                {
                    EdgeUpgrader.Instance.HideTemporaryEdge();
                }
                break;
            default:
                break;
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
                if (hit.transform == null || hit.transform.gameObject == EdgeSpawner.Instance.TemporaryEdge)
                {
                    EdgeSpawner.Instance.PlaceNode();
                }
                else
                {
                    PlantEdge edge = hit.transform.gameObject.GetComponent<PlantEdge>();
                    if (edge != null)
                    {
                        EdgeSpawner.Instance.StartEdgePlacement(start, edge);
                    }
                }
                break;
            case Tool.Upgrade:
                //Upgrade
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
