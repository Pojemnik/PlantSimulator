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

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveCamera = playerInput.actions.FindAction("MoveCamera", true);
    }

    private void Update()
    {
        cameraMovement.MoveCameraRelative(moveCamera.ReadValue<Vector2>() * Time.deltaTime);
    }
}
