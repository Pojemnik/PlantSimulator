using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSpawner : Singleton<NodeSpawner>
{
    [Header("References")]
    [SerializeField]
    private PlantNode plantCore;
    [SerializeField]
    private TemporaryEdgeController temporaryEdge;

    [Header("Config")]
    [SerializeField]
    private float minEgdeLength;

    [Header("Subnodes")]
    [SerializeField]
    private float distanceBetweenSubnodes;
    [SerializeField]
    private float offset;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject nodePrefab;
    [SerializeField]
    private GameObject subnodePrefab;
    [SerializeField]
    private GameObject edgePrefab;

    private bool nodePlacement;
    private Vector2 placementStartPosition;
    private Vector2 currentPosition;
    private PlantEdge.EdgeType newEdgeType;
    private PlantNodeBase placementStartNode;

    private void Start()
    {
        nodePlacement = false;
        temporaryEdge.gameObject.SetActive(false);
    }

    public void StartNodePlacement(PlantNodeBase startNode)
    {
        nodePlacement = true;
        placementStartNode = startNode;
        placementStartPosition = startNode.transform.position;
        temporaryEdge.edgeStart.transform.position = placementStartPosition;
        currentPosition = placementStartPosition;
        newEdgeType = startNode.edge.Type;
        temporaryEdge.gameObject.SetActive(true);
    }

    public void StopNodePlacement()
    {
        nodePlacement = false;
        temporaryEdge.gameObject.SetActive(false);
    }

    public void PlaceNode()
    {
        if (!nodePlacement)
        {
            return;
        }
        if (!CheckPlacementCorrectness(true))
        {
            return;
        }
        if (placementStartNode is SubNode)
        {
            SplitEdge();
        }
        PlantNode startNode = (PlantNode)placementStartNode;
        PlantNode newEndNode = Instantiate(nodePrefab).GetComponent<PlantNode>();
        newEndNode.transform.position = LayersManager.Instance.GetPositionOnLayer(currentPosition, LayersManager.LayerNames.Nodes);
        PlantEdge newEdge = CreateEdge(startNode, newEndNode);
        startNode.successors.Add(newEdge);
        newEndNode.edge = newEdge;
        newEndNode.GetComponent<CircleRenderer>().Calculate();
        PlaceSubnodes(newEdge);
        StopNodePlacement();
    }

    private void SplitEdge()
    {
        PlantNode createdNode = Instantiate(nodePrefab).GetComponent<PlantNode>();
        createdNode.transform.position = LayersManager.Instance.GetPositionOnLayer(placementStartNode.transform.position, LayersManager.LayerNames.Edges);
        PlantEdge edgeBefore = CreateEdge(placementStartNode.edge.begin, createdNode);
        PlantEdge edgeAfter = CreateEdge(createdNode, placementStartNode.edge.end);
        createdNode.edge = edgeBefore;
        createdNode.successors = new List<PlantEdge>(new PlantEdge[] { edgeAfter });
        TransferSubnodesToNewEdges(edgeBefore, edgeAfter);
        Destroy(placementStartNode.edge.gameObject);
        Destroy(placementStartNode.gameObject);
        placementStartNode = createdNode;
        createdNode.GetComponent<CircleRenderer>().Calculate();
    }

    private PlantEdge CreateEdge(PlantNode begin, PlantNode end)
    {
        PlantEdge edge = Instantiate(edgePrefab).GetComponent<PlantEdge>();
        edge.begin = begin;
        edge.Type = placementStartNode.edge.Type;
        edge.end = end;
        Vector3 startPosition = LayersManager.Instance.GetPositionOnLayer(edge.begin.transform.position, LayersManager.LayerNames.Edges);
        Vector3 endPosition = LayersManager.Instance.GetPositionOnLayer(edge.end.transform.position, LayersManager.LayerNames.Edges);
        edge.SetPositions(startPosition, endPosition);
        return edge;
    }

    private void TransferSubnodesToNewEdges(PlantEdge edgeBefore, PlantEdge edgeAfter)
    {
        float startNodeDistance = (placementStartNode.transform.position - placementStartNode.transform.position).magnitude;
        foreach (SubNode subNode in placementStartNode.edge.subnodes)
        {
            if (subNode == placementStartNode)
            {
                continue;
            }
            float subnodeDistance = (subNode.transform.position - placementStartNode.transform.position).magnitude;
            if (startNodeDistance > subnodeDistance)
            {
                edgeAfter.subnodes.Add(subNode);
                subNode.edge = edgeAfter;
            }
            else
            {
                edgeBefore.subnodes.Add(subNode);
                subNode.edge = edgeBefore;
            }
        }
    }

    private void PlaceSubnodes(PlantEdge edge)
    {
        Vector3 startPos = edge.begin.transform.position;
        Vector3 endPos = edge.end.transform.position;
        Vector3 edgeVector = endPos - startPos;
        float length = edgeVector.magnitude;
        edgeVector = edgeVector.normalized;
        length -= offset;
        int subnodesCount = (int)(length / distanceBetweenSubnodes);
        float additionalOffset = length - distanceBetweenSubnodes * (subnodesCount - 1);
        startPos += edgeVector * additionalOffset / 2;
        startPos += edgeVector * offset / 2;
        for (int i = 0; i < subnodesCount; i++)
        {
            SubNode subNode = Instantiate(subnodePrefab).GetComponent<SubNode>();
            subNode.transform.position = LayersManager.Instance.GetPositionOnLayer(startPos + edgeVector * distanceBetweenSubnodes * i, LayersManager.LayerNames.Nodes);
            subNode.edge = edge;
            subNode.GetComponent<CircleRenderer>().Calculate();
            edge.subnodes.Add(subNode);
        }
    }

    private bool CheckPlacementCorrectness(bool displayMessages)
    {
        if ((currentPosition - placementStartPosition).magnitude < minEgdeLength)
        {
            if(displayMessages)
            {
                Debug.Log("Plant edge too short");
            }
            return false;
        }
        if (currentPosition.y < plantCore.transform.position.y && newEdgeType == PlantEdge.EdgeType.Stem)
        {
            if(displayMessages)
            {
                Debug.Log("Stem cannot be underground");
            }
            return false;
        }
        if (currentPosition.y > plantCore.transform.position.y && newEdgeType == PlantEdge.EdgeType.Root)
        {
            if(displayMessages)
            {
                Debug.Log("Root have to be underground");
            }
            return false;
        }
        foreach(Collider2D collider in temporaryEdge.collidesWith)
        {
            if(collider.gameObject != placementStartNode.gameObject)
            {
                if(displayMessages)
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
        if (nodePlacement)
        {
            temporaryEdge.UpdateEdgePosition();
            temporaryEdge.PlacementCorrectness = CheckPlacementCorrectness(false);
        }
    }
}

