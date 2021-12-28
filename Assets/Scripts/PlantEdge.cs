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
        Wood,
        Temp
    }

    [SerializeField]
    private EdgeType type;
    public List<SubNode> subnodes;
    public PlantNode begin;
    public PlantNode end;

    private LineRenderer lineRenderer;

    public EdgeType Type
    {
        get => type; set
        {
            type = value;
            if(PlantColorsManager.Instance.gradients.TryGetValue(type, out Gradient gradient))
            {
                lineRenderer.colorGradient = gradient;
            }
            else
            {
                Debug.LogWarningFormat("No gradient for edge type {0}", type);
            }
        }
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
    }

    public void SetPositions(Vector3 begin, Vector3 end)
    {
        lineRenderer.SetPositions(new Vector3[] { begin, end });
    }

    public void SetGradient(Gradient gradient)
    {
        lineRenderer.colorGradient = gradient;
    }
}
