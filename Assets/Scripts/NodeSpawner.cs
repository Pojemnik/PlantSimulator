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
        if(!nodePlacement)
        {
            return;
        }
        if(!CheckPlacementCorrectness())
        {
            return;
        }
        GameObject newEdgeObject = Instantiate(edgePrefab);
        PlantEdge newEdge = newEdgeObject.GetComponent<PlantEdge>();
        newEdge.Type = newEdgeType;
        newEdge.SetPositions(placementStartPosition, currentPosition);
        //if(placementStartNode is SubNode)
        //{
        //    PlantNode createdNode = Instantiate(nodePrefab, placementStartNode.edge.transform).GetComponent<PlantNode>();
        //    createdNode.predecessor
        //}
        //placementStartNode.successors.Add(newEdge);
        GameObject newNodeObject = Instantiate(nodePrefab);
        newNodeObject.transform.position = currentPosition;
        PlantNode newNode = newNodeObject.GetComponent<PlantNode>();
        newNode.edge = newEdge;
        StopNodePlacement();
    }

    private bool CheckPlacementCorrectness()
    {
        if ((currentPosition - placementStartPosition).magnitude < minEgdeLength)
        {
            Debug.Log("Plant edge too short");
            return false;
        }
        if(currentPosition.y < plantCore.transform.position.y && newEdgeType == PlantEdge.EdgeType.Stem)
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
        if(nodePlacement)
        {
            temporaryEdge.UpdateEdgePositions();
            temporaryEdge.PlacementCorrectness = CheckPlacementCorrectness();
        }
    }
}

