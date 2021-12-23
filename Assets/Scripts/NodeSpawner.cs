using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSpawner : Singleton<NodeSpawner>
{
    [SerializeField]
    private PlantNode plantCore;
    [SerializeField]
    private TemporaryEdgeController temporaryEdge;
    [SerializeField]
    private float minEgdeLength;

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
    private PlantNode placementStartNode;

    private void Start()
    {
        nodePlacement = false;
        temporaryEdge.gameObject.SetActive(false);
    }

    public void StartNodePlacement(PlantNode startNode)
    {
        nodePlacement = true;
        placementStartNode = startNode;
        placementStartPosition = startNode.transform.position;
        temporaryEdge.edgeStart.transform.position = placementStartPosition;
        currentPosition = placementStartPosition;
        newEdgeType = startNode.predecessor.type;
        temporaryEdge.gameObject.SetActive(true);
    }

    public void StopNodePlacement()
    {
        nodePlacement = false;
        temporaryEdge.gameObject.SetActive(false);
    }

    public void PlaceNode()
    {
        if(!CheckPlacementCorrectness())
        {
            return;
        }
        //GameObject newEdge = Instantiate(edgePrefab, placementStartNode.transform);
        //Instantiate(nodePrefab, newEdge.transform);
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

