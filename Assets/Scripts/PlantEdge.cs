using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlantEdge : MonoBehaviour
{
    public enum EdgeType
    {
        Stem,
        Root,
        Wood
    }

    public EdgeType type;
    public List<SubNode> subnodes;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetPositions(Vector2 begin, Vector2 end)
    {
        lineRenderer.SetPositions(new Vector3[] { begin, end });
    }
}
