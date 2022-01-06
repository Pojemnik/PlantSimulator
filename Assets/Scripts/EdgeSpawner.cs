using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeSpawner : Singleton<EdgeSpawner>
{
    [Header("References")]
    [SerializeField]
    private PlantNode plantCore;
    [SerializeField]
    private TemporaryEdgeController temporaryEdge;
    public GameObject TemporaryEdge
    {
        get
        {
            return temporaryEdge.gameObject;
        }
    }

    [Header("Config")]
    [SerializeField]
    private float minEgdeLength;
    [SerializeField]
    private float nodeEndRadius;
    [SerializeField]
    private float minAngle;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject nodePrefab;
    [SerializeField]
    private GameObject edgePrefab;

    private bool edgePlacement;
    private bool placementAtEndNode;
    private Vector2 placementStartPosition;
    private Vector2 currentPosition;
    private PlantEdge.EdgeType newEdgeType;
    private PlantEdge placementStartEdge;
    private PlantNode newEdgeBeginNode;
    private HashSet<Collider2D> startCollision;

    private void Start()
    {
        edgePlacement = false;
        temporaryEdge.gameObject.SetActive(false);
        startCollision = new HashSet<Collider2D>();
    }

    public void StartEdgePlacement(Vector2 position, PlantEdge edge)
    {
        placementStartEdge = edge;
        TestAndInitPlacementAtEndNode(position, edge);
        edgePlacement = true;
        temporaryEdge.edgeStart.transform.position = placementStartPosition;
        currentPosition = placementStartPosition;
        newEdgeType = placementStartEdge.Type;
        temporaryEdge.gameObject.SetActive(true);
        temporaryEdge.UpdateEdgePosition();
        temporaryEdge.PlacementCorrectness = CheckPlacementCorrectness(false);
        temporaryEdge.Level = PlantConfigManager.Instance.defaultEgdeWidths[newEdgeType];
    }

    private void TestAndInitPlacementAtEndNode(Vector2 position, PlantEdge edge)
    {
        Vector3 edgeVector = edge.end.transform.position - edge.begin.transform.position;
        placementStartPosition = Vector3.Project((Vector3)position - edge.begin.transform.position, edgeVector);
        placementStartPosition += (Vector2)edge.begin.transform.position;
        float beginDist = Vector2.Distance(placementStartPosition, edge.begin.transform.position);
        float endDist = Vector2.Distance(placementStartPosition, edge.end.transform.position);
        if (beginDist < endDist)
        {
            if (beginDist < nodeEndRadius)
            {
                placementStartEdge = edge.begin.edge;
                placementAtEndNode = true;
                newEdgeBeginNode = edge.begin;
                placementStartPosition = newEdgeBeginNode.transform.position;
                Debug.LogFormat("Placement at the end of {0} - {1} (form begining)", placementStartEdge, newEdgeBeginNode);
            }
            else
            {
                placementAtEndNode = false;
            }
        }
        else
        {
            if (endDist < nodeEndRadius)
            {
                placementAtEndNode = true;
                newEdgeBeginNode = edge.end;
                placementStartPosition = newEdgeBeginNode.transform.position;
                Debug.LogFormat("Placement at the end of {0} - {1}", placementStartEdge, newEdgeBeginNode);
            }
            else
            {
                placementAtEndNode = false;
            }
        }
    }

    public void StopNodePlacement()
    {
        edgePlacement = false;
        temporaryEdge.gameObject.SetActive(false);
    }

    public void PlaceNode()
    {
        if (!edgePlacement)
        {
            return;
        }
        if (!CheckPlacementCorrectness(true))
        {
            return;
        }
        if (!placementAtEndNode)
        {
            SplitEdge();
        }
        PlantNode startNode = newEdgeBeginNode;
        PlantNode newEndNode = Instantiate(nodePrefab).GetComponent<PlantNode>();
        newEndNode.transform.position = LayersManager.Instance.GetPositionOnLayer(currentPosition, LayersManager.LayerNames.Nodes);
        PlantEdge newEdge = CreateEdge(startNode, newEndNode);
        startNode.successors.Add(newEdge);
        newEndNode.edge = newEdge;
        newEndNode.GetComponent<CircleRenderer>().Calculate();
        newEdge.Level = temporaryEdge.Level;
        StopNodePlacement();
    }

    private void SplitEdge()
    {
        PlantNode createdNode = Instantiate(nodePrefab).GetComponent<PlantNode>();
        createdNode.transform.position = LayersManager.Instance.GetPositionOnLayer(placementStartPosition, LayersManager.LayerNames.Edges);
        PlantEdge edgeBefore = CreateEdge(placementStartEdge.begin, createdNode);
        PlantEdge edgeAfter = CreateEdge(createdNode, placementStartEdge.end);
        createdNode.edge = edgeBefore;
        createdNode.successors = new List<PlantEdge>(new PlantEdge[] { edgeAfter });
        Destroy(placementStartEdge.gameObject);
        newEdgeBeginNode = createdNode;
        createdNode.GetComponent<CircleRenderer>().Calculate();
    }

    private PlantEdge CreateEdge(PlantNode begin, PlantNode end)
    {
        PlantEdge edge = Instantiate(edgePrefab).GetComponent<PlantEdge>();
        edge.begin = begin;
        edge.Type = placementStartEdge.Type;
        edge.end = end;
        edge.Level = placementStartEdge.Level;
        edge.gameObject.layer = LayerMask.NameToLayer("Edges");
        Vector3 startPosition = LayersManager.Instance.GetPositionOnLayer(edge.begin.transform.position, LayersManager.LayerNames.Edges);
        Vector3 endPosition = LayersManager.Instance.GetPositionOnLayer(edge.end.transform.position, LayersManager.LayerNames.Edges);
        edge.SetPositions(startPosition, endPosition);
        edge.UpdateCollider();
        return edge;
    }

    private bool CheckPlacementCorrectness(bool displayMessages)
    {
        if ((currentPosition - placementStartPosition).magnitude < minEgdeLength)
        {
            if (displayMessages)
            {
                Debug.Log("Plant edge too short");
            }
            return false;
        }
        if (currentPosition.y < plantCore.transform.position.y && newEdgeType == PlantEdge.EdgeType.Stem)
        {
            if (displayMessages)
            {
                Debug.Log("Stem cannot be underground");
            }
            return false;
        }
        if (currentPosition.y > plantCore.transform.position.y && newEdgeType == PlantEdge.EdgeType.Root)
        {
            if (displayMessages)
            {
                Debug.Log("Root have to be underground");
            }
            return false;
        }
        Vector2 newEdgeVector = currentPosition - placementStartPosition;
        Vector2 startEdgeVector;
        if (placementAtEndNode)
        {
            startEdgeVector = (Vector2)placementStartEdge.end.transform.position - (Vector2)placementStartEdge.begin.transform.position;
        }
        else
        {
            startEdgeVector = (Vector2)placementStartEdge.end.transform.position - placementStartPosition;
        }
        float angle = Vector2.Angle(newEdgeVector, startEdgeVector);
        if((placementAtEndNode && 180 - angle < minAngle) || (!placementAtEndNode && Mathf.Min(angle, 180 - angle) < minAngle))
        {
            if (displayMessages)
            {
                Debug.Log("Angle between the new and the old enge is too small");
            }
            return false;
        }
        foreach (Collider2D collider in temporaryEdge.collidesWith)
        {
            if (collider.gameObject != placementStartEdge.gameObject)
            {
                if (displayMessages)
                {
                    Debug.Log("Edge collides with something");
                }
                return false;
            }
        }
        return true;
    }

    public void OnSelectionMove(Vector2 position)
    {
        temporaryEdge.edgeEnd.transform.position = position;
        currentPosition = position;
        if (edgePlacement)
        {
            temporaryEdge.UpdateEdgePosition();
            temporaryEdge.PlacementCorrectness = CheckPlacementCorrectness(false);
        }
    }
}

