using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlantNode : MonoBehaviour, IInteractive
{
    public PlantEdge predecessor;
    public List<PlantEdge> successors;

    public void OnInteraction(Vector2 position)
    {
        Debug.LogFormat("Interaction with PlantNode {0} on position {1}", gameObject.name, position);
    }
}
