using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryEdgeController : MonoBehaviour
{
    public PlantNode edgeStart;
    public PlantNode edgeEnd;
    public PlantEdge edge;

    public void UpdateEdgePositions()
    {
        edge.SetPositions(edgeStart.transform.position, edgeEnd.transform.position);
    }
}
