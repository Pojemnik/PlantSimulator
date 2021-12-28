using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlantNode : PlantNodeBase, IInteractive
{
    public List<PlantEdge> successors;
}
