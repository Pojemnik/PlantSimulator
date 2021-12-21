using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantNode : MonoBehaviour, IInteractive
{
    private PlantEdge predecessor;
    private List<PlantEdge> successors;

    public void OnInteraction(Vector2 position)
    {
        Debug.LogFormat("Interaction with PlantNode {0} on position {1}", gameObject.name, position);
    }
}
