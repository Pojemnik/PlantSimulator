using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed;
    [SerializeField]
    private float zoomSpeed;
    [SerializeField]
    private float minZoom;
    [SerializeField]
    private float maxZoom;

    public void MoveCameraRelative(Vector2 delta, bool ignoreSpeed = false)
    {
        if (ignoreSpeed)
        {
            transform.position = transform.position + (Vector3)delta;
        }
        else
        {
            transform.position = transform.position + (Vector3)delta * cameraSpeed;
        }
    }

    public void MoveCameraToPosition(Vector2 position)
    {
        Vector3 newPos = position;
        newPos.z = transform.position.z;
        transform.position = newPos;
    }

    public void ZoomCamera(float diff)
    {
        Camera.main.orthographicSize += diff * zoomSpeed;
        if(Camera.main.orthographicSize < minZoom)
        {
            Camera.main.orthographicSize = minZoom;
        }
        else
        {
            if (Camera.main.orthographicSize > maxZoom)
            {
                Camera.main.orthographicSize = maxZoom;
            }
        }
    }
}
