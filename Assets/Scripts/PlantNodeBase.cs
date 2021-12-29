using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlantNodeBase : MonoBehaviour, IInteractive
{
    public PlantEdge edge;

    public void OnInteraction(Vector2 position)
    {
        NodeSpawner.Instance.StartNodePlacement(this);
    }
}