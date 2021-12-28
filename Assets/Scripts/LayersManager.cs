using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayersManager : Singleton<LayersManager>
{
    public enum LayerNames
    {
        Edges,
        Nodes
    }

    [SerializeField]
    private float edgesZ;
    [SerializeField]
    private float nodesZ;

    public Vector3 GetPositionOnLayer(Vector2 position, LayerNames layer)
    {
        Vector3 positionOnLayer = position;
        switch(layer)
        {
            case LayerNames.Edges:
                positionOnLayer.z = edgesZ;
                break;
            case LayerNames.Nodes:
                positionOnLayer.z = nodesZ;
                break;
            default:
                Debug.LogWarningFormat("Incorrect layer in request to layers manager: {0}", layer);
                break;
        }
        return positionOnLayer;
    }
}
