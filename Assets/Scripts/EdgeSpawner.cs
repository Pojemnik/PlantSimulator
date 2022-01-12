using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class EdgeSpawner : Singleton<EdgeSpawner>
{
    [Header("References")]
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
    private Vector2 currentNewEndNodePosition;
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
        Vector2 edgeVector = edge.end.transform.position - edge.begin.transform.position;
        placementStartPosition = Vector3.Project(position - (Vector2)edge.begin.transform.position, edgeVector);
        placementStartPosition += (Vector2)edge.begin.transform.position;
        TestAndInitPlacementAtEndNode();
        edgePlacement = true;
        temporaryEdge.edge.begin.transform.position = placementStartPosition;
        currentNewEndNodePosition = placementStartPosition;
        newEdgeType = placementStartEdge.Type;
        temporaryEdge.gameObject.SetActive(true);
        temporaryEdge.UpdateEdgePosition();
        temporaryEdge.PlacementCorrectness = CheckPlacementCorrectness(false);
        temporaryEdge.Level = PlantConfigManager.Instance.defaultEgdeWidths[newEdgeType];
    }

    private void TestAndInitPlacementAtEndNode()
    {
        collidingEdgesInfo.Clear();
        float testZoneRadius = PlantConfigManager.Instance.edgeWidthsOnLevels[placementStartEdge.Level] / 2;
        GetStartCollision(testZoneRadius);
        HashSet<PlantNode> closestNodesOnCollidingEdges = TestPlacementAtEndNode(testZoneRadius);
        if (placementAtEndNode)
        {
            InitPlacementAtEndNode(closestNodesOnCollidingEdges.ElementAt(0));
        }
    }

    private HashSet<PlantNode> TestPlacementAtEndNode(float testZoneRadius)
    {
        placementAtEndNode = false;
        HashSet<PlantNode> closestNodesOnCollidingEdges = new HashSet<PlantNode>();
        if (startCollision.Count > 1)
        {
            HashSet<PlantEdge> collidingEdges = startCollision.Select((e) => e.gameObject.GetComponent<PlantEdge>()).ToHashSet();
            foreach (PlantEdge collidingEdge in collidingEdges)
            {
                float distanceToEdgeBegin = Vector2.Distance(collidingEdge.begin.transform.position, placementStartPosition);
                float distanceToEdgeEnd = Vector2.Distance(collidingEdge.end.transform.position, placementStartPosition);
                if (distanceToEdgeBegin < distanceToEdgeEnd)
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
            if (closestNodesOnCollidingEdges.Count == 1)
            {
                placementAtEndNode = true;
            }
            else
            {
                PlantNode node = SelectClosestNode(closestNodesOnCollidingEdges);
                closestNodesOnCollidingEdges.Clear();
                closestNodesOnCollidingEdges.Add(node);
                placementAtEndNode = true;
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
                    placementAtEndNode = true;
                }
            }
            else
            {
                Debug.LogError("No collision found at node placement. This should never happen");
            }
        }
        return closestNodesOnCollidingEdges;
    }

    private PlantNode SelectClosestNode(HashSet<PlantNode> closestNodesOnCollidingEdges)
    {
        PlantNode node = GetClosestNode(closestNodesOnCollidingEdges);
        List<(PlantEdge, CollidingEdgePart)> edgesCollidingWithSelectedNode = new List<(PlantEdge, CollidingEdgePart)>();
        foreach ((PlantEdge, CollidingEdgePart) edge in collidingEdgesInfo)
        {
            if (edge.Item1.begin == node || edge.Item1.end == node)
            {
                edgesCollidingWithSelectedNode.Add(edge);
            }
        }
        collidingEdgesInfo = edgesCollidingWithSelectedNode;
        return node;
    }

    private PlantNode GetClosestNode(HashSet<PlantNode> closestNodesOnCollidingEdges)
    {
        PlantNode node = closestNodesOnCollidingEdges.ElementAt(0);
        float dist = Vector2.Distance(node.transform.position, placementStartPosition);
        foreach (PlantNode n in closestNodesOnCollidingEdges)
        {
            float nDist = Vector2.Distance(n.transform.position, placementStartPosition);
            if (nDist < dist)
            {
                dist = nDist;
                node = n;
            }
        }
        return node;
    }

    private void InitPlacementAtEndNode(PlantNode startNode)
    {
        newEdgeBeginNode = startNode;
        placementStartPosition = newEdgeBeginNode.transform.position;
    }

    private void GetStartCollision(float testZoneRadius)
    {
        int layerMask = LayerMask.GetMask("Edges");
        startCollision = new HashSet<Collider2D>(Physics2D.OverlapCircleAll(placementStartPosition, testZoneRadius, layerMask));
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
        newEndNode.transform.position = LayersManager.Instance.GetPositionOnLayer(currentNewEndNodePosition, LayersManager.LayerNames.Nodes);
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
        if ((currentNewEndNodePosition - placementStartPosition).magnitude < minEgdeLength)
        {
            if (displayMessages)
            {
                Debug.Log("Plant edge too short");
            }
            return false;
        }
        if (currentNewEndNodePosition.y < PlantConfigManager.Instance.minStemHeight && newEdgeType == PlantEdge.EdgeType.Stem)
        {
            if (displayMessages)
            {
                Debug.Log("Stem cannot be underground");
            }
            return false;
        }
        if (currentNewEndNodePosition.y > PlantConfigManager.Instance.maxRootHeight && newEdgeType == PlantEdge.EdgeType.Root)
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
        Vector2 newEdgeVector = currentNewEndNodePosition - placementStartPosition;
        if (placementAtEndNode)
        {
            bool angleCheck = AngleCheckAtEndNode(newEdgeVector);
            if (!angleCheck)
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
            bool angleCheck = AngleCheckAtRegularNode(newEdgeVector);
            if (!angleCheck)
            {
                if (displayMessages)
                {
                    Debug.Log("Angle between the new and the old edge is too small");
                }
                return false;
            }
        }
        return true;
    }

    private bool AngleCheckAtRegularNode(Vector2 newEdgeVector)
    {
        Vector2 startEdgeVector = (Vector2)placementStartEdge.end.transform.position - placementStartPosition;
        float angle = Vector2.Angle(newEdgeVector, startEdgeVector);
        return Mathf.Min(angle, 180 - angle) >= minAngle;
    }

    private bool AngleCheckAtEndNode(Vector2 newEdgeVector)
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
                return false;
            }
        }
        return true;
    }

    public void OnSelectionMove(Vector2 position)
    {
        temporaryEdge.edge.end.transform.position = position;
        currentNewEndNodePosition = position;
        if (edgePlacement)
        {
            temporaryEdge.UpdateEdgePosition();
            temporaryEdge.PlacementCorrectness = CheckPlacementCorrectness(false);
        }
    }
}

