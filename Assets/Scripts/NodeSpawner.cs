using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSpawner : Singleton<NodeSpawner>
{
    [SerializeField]
    private PlantNode plantCore;
    [SerializeField]
    private TemporaryEdgeController temporaryEdge;

    [Header("Plant base config")]
    [Header("Stem node")]
    [SerializeField]
    private Vector2 stemTop;
    [Header("Root node")]
    [SerializeField]
    private Vector2 rootBottom;

    private bool nodePlacement;

    private void Start()
    {
        nodePlacement = false;
        temporaryEdge.gameObject.SetActive(false);
    }

    public void StartNodePlacement(PlantNode startPos)
    {
        nodePlacement = true;
        temporaryEdge.edgeStart.transform.position = startPos.transform.position;
        temporaryEdge.gameObject.SetActive(true);
    }

    public void StopNodePlacement()
    {
        nodePlacement = false;
        temporaryEdge.gameObject.SetActive(false);
    }

    public void OnSelectionMove(Vector2 position)
    {
        temporaryEdge.edgeEnd.transform.position = position;
        if(nodePlacement)
        {
            temporaryEdge.UpdateEdgePositions();
        }
    }
}

