using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed;

    public void MoveCameraRelative(Vector2 delta)
    {
        transform.position = transform.position + (Vector3)delta * cameraSpeed;
    }

    public void MoveCameraToPosition(Vector2 position)
    {
        Vector3 newPos = position;
        newPos.z = transform.position.z;
        transform.position = newPos;
    }
}
