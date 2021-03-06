using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantNode : PlantNodeBase, IInteractive
{
    public List<PlantEdge> successors;

    protected override void Start()
    {
        base.Start();
        gameObject.name = string.Format("Plant node {0}", PlantConfigManager.Instance.NodeCounter);
    }
}
