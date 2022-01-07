using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    private List<(PlantEdge, CollidingEdgePart)> collidingEdgesInfo;

    private enum CollidingEdgePart
    {
        Begin,
        End
    }

    private void Start()
    {
        edgePlacement = false;
        temporaryEdge.gameObject.SetActive(false);
        startCollision = new HashSet<Collider2D>();
        collidingEdgesInfo = new List<(PlantEdge, CollidingEdgePart)>();
    }

    public void StartEdgePlacement(Vector2 position, PlantEdge edge)
    {
        if (edgePlacement)
        {
            return;
        }
        placementStartEdge = edge;
        Vector3 edgeVector = edge.end.transform.position - edge.begin.transform.position;
        placementStartPosition = Vector3.Project((Vector3)position - edge.begin.transform.position, edgeVector);
        placementStartPosition += (Vector2)edge.begin.transform.position;
        TestAndInitPlacementAtEndNode();
        edgePlacement = true;
        temporaryEdge.edgeStart.transform.position = placementStartPosition;
        currentPosition = placementStartPosition;
        newEdgeType = placementStartEdge.Type;
        temporaryEdge.gameObject.SetActive(true);
        temporaryEdge.UpdateEdgePosition();
        temporaryEdge.PlacementCorrectness = CheckPlacementCorrectness(false);
        temporaryEdge.Level = PlantConfigManager.Instance.defaultEgdeWidths[newEdgeType];
    }

    private void TestAndInitPlacementAtEndNode()
    {
        collidingEdgesInfo.Clear();
        float testZoneRadius = PlantConfigManager.Instance.edgeWidthsOnLevels[placementStartEdge.Level];
        int layerMask = LayerMask.GetMask("Edges");
        startCollision = new HashSet<Collider2D>(Physics2D.OverlapCircleAll(placementStartPosition, testZoneRadius, layerMask));
        foreach (Collider2D col in startCollision)
        {
            Debug.Log(col.gameObject.name);
        }
        HashSet<PlantNode> closestNodesOnCollidingEdges = new HashSet<PlantNode>();
        if (startCollision.Count > 1)
        {
            HashSet<PlantEdge> collidingEdges = startCollision.Select((e) => e.gameObject.GetComponent<PlantEdge>()).ToHashSet();
            foreach (PlantEdge collidingEdge in collidingEdges)
            {
                float beginDist = Vector2.Distance(collidingEdge.begin.transform.position, placementStartPosition);
                float endDist = Vector2.Distance(collidingEdge.end.transform.position, placementStartPosition);
                if (beginDist < endDist)
                {
                    closestNodesOnCollidingEdges.Add(collidingEdge.begin);
                    collidingEdgesInfo.Add((collidingEdge, CollidingEdgePart.Begin));
                }
                else
                {
                    closestNodesOnCollidingEdges.Add(collidingEdge.end);
                    collidingEdgesInfo.Add((collidingEdge, CollidingEdgePart.End));
                }
            }
            if (closestNodesOnCollidingEdges.Count != 1)
            {
                Debug.LogError("Incorrect start nodes of colliding edges");
                foreach (PlantNode node in closestNodesOnCollidingEdges)
                {
                    Debug.Log(node.gameObject.name);
                }
                return;
            }
        }
        else
        {
            if (startCollision.Count == 1)
            {
                PlantEdge collidingEdge = startCollision.ElementAt(0).GetComponent<PlantEdge>();
                float endDist = Vector2.Distance(collidingEdge.end.transform.position, placementStartPosition);
                if (endDist < testZoneRadius)
                {
                    closestNodesOnCollidingEdges.Add(collidingEdge.end);
                    collidingEdgesInfo.Add((collidingEdge, CollidingEdgePart.End));
                }
                else
                {
                    return;
                }
            }
            else
            {
                Debug.LogError("No collision found at node placement. This should never happen");
                return;
            }
        }
        //Place at the end node
        Debug.LogFormat("End node placement");
        PlantNode startNode = closestNodesOnCollidingEdges.ElementAt(0);
        placementAtEndNode = true;
        newEdgeBeginNode = startNode;
        placementStartPosition = newEdgeBeginNode.transform.position;
        Debug.LogFormat("Placement at the end of {0} - {1}", placementStartEdge, newEdgeBeginNode);
        foreach (var x in collidingEdgesInfo)
        {
            Debug.Log(x);
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
        if (!temporaryEdge.collidesWith.IsSubsetOf(startCollision))
        {
            if (displayMessages)
            {
                Debug.Log("Edge collides with something");
            }
            return false;
        }
        Vector2 newEdgeVector = currentPosition - placementStartPosition;
        if (!placementAtEndNode)
        {
            Vector2 startEdgeVector = (Vector2)placementStartEdge.end.transform.position - placementStartPosition;
            float angle = Vector2.Angle(newEdgeVector, startEdgeVector);
            if (!placementAtEndNode && Mathf.Min(angle, 180 - angle) < minAngle)
            {
                if (displayMessages)
                {
                    Debug.Log("Angle between the new and the old edge is too small");
                }
                return false;
            }
        }
        else
        {
            foreach ((PlantEdge edge, CollidingEdgePart part) in collidingEdgesInfo)
            {
                Vector2 startEdgeVector;
                if (part == CollidingEdgePart.Begin)
                {
                    startEdgeVector = (Vector2)edge.end.transform.position - (Vector2)edge.begin.transform.position;
                }
                else
                {
                    startEdgeVector = (Vector2)edge.begin.transform.position - (Vector2)edge.end.transform.position;
                }
                float angle = Vector2.Angle(newEdgeVector, startEdgeVector);
                if (angle < minAngle)
                {
                    if (displayMessages)
                    {
                        Debug.Log("Angle between the new and the old edge is too small");
                    }
                    return false;
                }
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

