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
    [SerializeField]
    private float distanceBetweenSubnodes;

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
        if (!CheckPlacementCorrectness())
        {
            return;
        }
        GameObject newEdgeObject = Instantiate(edgePrefab);
        PlantEdge newEdge = newEdgeObject.GetComponent<PlantEdge>();
        newEdge.Type = newEdgeType;
        newEdge.SetPositions(placementStartPosition, currentPosition);
        if (placementStartNode is SubNode)
        {
            SplitEdge();
        }
        PlantNode startNode = (PlantNode)placementStartNode;
        startNode.successors.Add(newEdge);
        PlantNode newEndNode = Instantiate(nodePrefab).GetComponent<PlantNode>();
        newEndNode.transform.position = LayersManager.Instance.GetPositionOnLayer(currentPosition, LayersManager.LayerNames.Nodes);
        newEndNode.edge = newEdge;
        newEndNode.GetComponent<CircleRenderer>().Calculate();
        newEdge.begin = startNode;
        newEdge.end = newEndNode;
        PlaceSubnodes(newEdge);
        StopNodePlacement();
    }

    private void SplitEdge()
    {
        //Subnodes should be transfered to new edges instead of being removed and created
        PlantNode createdNode = Instantiate(nodePrefab).GetComponent<PlantNode>();
        createdNode.transform.position = LayersManager.Instance.GetPositionOnLayer(placementStartNode.transform.position, LayersManager.LayerNames.Edges);
        PlantEdge edgeBefore = Instantiate(edgePrefab).GetComponent<PlantEdge>();
        edgeBefore.begin = placementStartNode.edge.begin;
        edgeBefore.Type = placementStartNode.edge.Type;
        edgeBefore.end = createdNode;
        Vector3 startPosition = LayersManager.Instance.GetPositionOnLayer(edgeBefore.begin.transform.position, LayersManager.LayerNames.Edges);
        Vector3 endPosition = LayersManager.Instance.GetPositionOnLayer(edgeBefore.end.transform.position, LayersManager.LayerNames.Edges);
        edgeBefore.SetPositions(startPosition, endPosition);
        PlaceSubnodes(edgeBefore);
        PlantEdge edgeAfter = Instantiate(edgePrefab).GetComponent<PlantEdge>();
        edgeAfter.begin = createdNode;
        edgeAfter.Type = placementStartNode.edge.Type;
        edgeAfter.end = placementStartNode.edge.end;
        startPosition = LayersManager.Instance.GetPositionOnLayer(edgeAfter.begin.transform.position, LayersManager.LayerNames.Edges);
        endPosition = LayersManager.Instance.GetPositionOnLayer(edgeAfter.end.transform.position, LayersManager.LayerNames.Edges);
        edgeAfter.SetPositions(startPosition, endPosition);
        PlaceSubnodes(edgeAfter);
        createdNode.edge = edgeBefore;
        createdNode.successors = new List<PlantEdge>(new PlantEdge[] { edgeAfter });
        foreach(SubNode subNode in placementStartNode.edge.subnodes)
        {
            Destroy(subNode.gameObject);
        }
        placementStartNode.edge.subnodes.Clear();
        Destroy(placementStartNode.edge.gameObject);
        Destroy(placementStartNode.gameObject);
        placementStartNode = createdNode;
        createdNode.GetComponent<CircleRenderer>().Calculate();
    }

    private void PlaceSubnodes(PlantEdge edge)
    {
        Vector3 startPos = edge.begin.transform.position;
        Vector3 endPos = edge.end.transform.position;
        Vector3 edgeVector = endPos - startPos;
        float length = edgeVector.magnitude;
        edgeVector = edgeVector.normalized;
        length -= 0.8f;
        int subnodesCount = (int)(length / 0.45f);
        float distanceBetweenSubnodes = length / subnodesCount;
        for (int i = 1; i <= subnodesCount; i++)
        {
            SubNode subNode = Instantiate(subnodePrefab).GetComponent<SubNode>();
            subNode.transform.position = startPos + edgeVector * distanceBetweenSubnodes * i;
            subNode.edge = edge;
            subNode.GetComponent<CircleRenderer>().Calculate();
            edge.subnodes.Add(subNode);
        }
    }

    private bool CheckPlacementCorrectness()
    {
        if ((currentPosition - placementStartPosition).magnitude < minEgdeLength)
        {
            Debug.Log("Plant edge too short");
            return false;
        }
        if (currentPosition.y < plantCore.transform.position.y && newEdgeType == PlantEdge.EdgeType.Stem)
        {
            Debug.Log("Stem cannot be underground");
            return false;
        }
        if (currentPosition.y > plantCore.transform.position.y && newEdgeType == PlantEdge.EdgeType.Root)
        {
            Debug.Log("Root have to be underground");
            return false;
        }
        return true;
    }

    public void OnSelectionMove(Vector2 position)
    {
        temporaryEdge.edgeEnd.transform.position = position;
        currentPosition = position;
        if (nodePlacement)
        {
            temporaryEdge.UpdateEdgePositions();
            temporaryEdge.PlacementCorrectness = CheckPlacementCorrectness();
        }
    }
}

