using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryEdgeController : MonoBehaviour
{
    public PlantNode edgeStart;
    public PlantNode edgeEnd;
    public PlantEdge edge;

    [SerializeField]
    private Gradient defaultGradient;
    [SerializeField]
    private Gradient errorGradient;

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
        collidesWith = new HashSet<Collider2D>();
        edge.TriggerEnter += (_, c) => collidesWith.Add(c);
        edge.TriggerExit += (_, c) => collidesWith.Remove(c);
    }

    public void UpdateEdgePosition()
    {
        edge.SetPositions(edgeStart.transform.position, edgeEnd.transform.position);
        edge.UpdateCollider();
    }
}
