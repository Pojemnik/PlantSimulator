using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class PlantEdge : MonoBehaviour, IInteractive
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


    public event System.EventHandler<Collider2D> TriggerEnter;
    public event System.EventHandler<Collider2D> TriggerExit;

    [SerializeField]
    private int level;
    public int Level
    {
        get => level;
        set
        {
            level = value;
            lineRenderer.endWidth = lineRenderer.startWidth = PlantConfigManager.Instance.edgeWidthsOnLevels[level];
            UpdateCollider();
        }
    }

    private new PolygonCollider2D collider;
    private LineRenderer lineRenderer;

    public EdgeType Type
    {
        get => type; set
        {
            type = value;
            if (PlantConfigManager.Instance.gradients.TryGetValue(type, out Gradient gradient))
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
        collider = GetComponent<PolygonCollider2D>();
    }

    public void SetPositions(Vector3 beginPos, Vector3 endPos)
    {
        lineRenderer.SetPositions(new Vector3[] { beginPos, endPos });
    }

    public void UpdateCollider()
    {
        float width = lineRenderer.startWidth;
        Vector2 normal = begin.transform.position - end.transform.position;
        normal = Vector2.Perpendicular(normal).normalized;
        Vector2[] colliderPoints = new Vector2[]
        {
            begin.transform.position + (Vector3)normal * width/2,
            end.transform.position + (Vector3)normal * width/2,
            end.transform.position - (Vector3)normal * width/2,
            begin.transform.position - (Vector3)normal * width/2
        };
        collider.points = colliderPoints;
    }

    public void SetGradient(Gradient gradient)
    {
        lineRenderer.colorGradient = gradient;
    }

    public void OnInteraction(Vector2 position)
    {
        EdgeSpawner.Instance.StartEdgePlacement(position, this);
    }

    public void OnHoverStart()
    {
    }

    public void OnHoverEnd()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Edges"))
        {
            TriggerEnter?.Invoke(this, collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Edges"))
        {
            TriggerExit?.Invoke(this, collision);
        }
    }
}
