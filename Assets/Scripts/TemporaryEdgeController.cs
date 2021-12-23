using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryEdgeController : MonoBehaviour
{
    public PlantNode edgeStart;
    public PlantNode edgeEnd;
    public PlantEdge edge;

    [SerializeField]
    private Gradient defaultGradient;
    [SerializeField]
    private Gradient errorGradient;

    private bool placementCorrectness;
    public bool PlacementCorrectness
    {
        get => placementCorrectness;
        set
        {
            placementCorrectness = value;
            edge.SetGradient(placementCorrectness ? defaultGradient : errorGradient);
        }
    }

    public void UpdateEdgePositions()
    {
        edge.SetPositions(edgeStart.transform.position, edgeEnd.transform.position);
    }
}
