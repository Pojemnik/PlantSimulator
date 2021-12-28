using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlantNodeBase : MonoBehaviour, IInteractive
{
    public PlantEdge edge;

    public void OnInteraction(Vector2 position)
    {
        Debug.LogFormat("Interaction with PlantNode {0} on position {1}", gameObject.name, position);
        NodeSpawner.Instance.StartNodePlacement(this);
    }
}