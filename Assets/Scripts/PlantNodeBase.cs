using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(CircleRenderer))]
public class PlantNodeBase : MonoBehaviour, IInteractive
{
    public PlantEdge edge;

    private CircleRenderer circle;

    private void Awake()
    {
        circle = GetComponent<CircleRenderer>();
    }

    private void Start()
    {
        circle.ShowCircle = false;
    }

    public void OnInteraction()
    {
        NodeSpawner.Instance.StartNodePlacement(this);
    }

    public void OnHoverStart()
    {
        circle.ShowCircle = true;
    }

    public void OnHoverEnd()
    {
        circle.ShowCircle = false;
    }
}