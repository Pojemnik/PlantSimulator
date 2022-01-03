using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class TemporaryEdgeController : MonoBehaviour
{
    public PlantNode edgeStart;
    public PlantNode edgeEnd;
    public PlantEdge edge;

    [SerializeField]
    private Gradient defaultGradient;
    [SerializeField]
    private Gradient errorGradient;
    [SerializeField]
    private float width;

    private new PolygonCollider2D collider;
    public HashSet<Collider2D> collidesWith { get; private set; }

    private bool placementCorrectness;
    public bool PlacementCorrectness
    {
        get => placementCorrectness;
        set
        {
            placementCorrectness = value;
            edge.SetGradient(placementCorrectness ? defaultGradient : errorGradient);
        }
    }

    private int level;
    public int Level
    {
        get => level;
        set
        {
            level = value;
            edge.Level = level;
        }
    }

    private void Awake()
    {
        collider = GetComponent<PolygonCollider2D>();
        collidesWith = new HashSet<Collider2D>();
    }

    public void UpdateEdgePosition()
    {
        edge.SetPositions(edgeStart.transform.position, edgeEnd.transform.position);
        Vector2 normal = edgeStart.transform.position - edgeEnd.transform.position;
        normal = Vector2.Perpendicular(normal).normalized;
        Vector2[] colliderPoints = new Vector2[]
        {
            edgeStart.transform.position + (Vector3)normal * width/2,
            edgeEnd.transform.position + (Vector3)normal * width/2,
            edgeEnd.transform.position - (Vector3)normal * width/2,
            edgeStart.transform.position - (Vector3)normal * width/2
        };
        collider.points = colliderPoints;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        collidesWith.Add(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        collidesWith.Remove(collision);
    }
}
