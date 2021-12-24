using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantColorsManager : Singleton<PlantColorsManager>
{
    [SerializeField]
    private Gradient stemGradient;
    [SerializeField]
    private Gradient rootGradient;
    [SerializeField]
    private Gradient woodGradient;

    public Dictionary<PlantEdge.EdgeType, Gradient> gradients;

    private void Awake()
    {
        gradients = new Dictionary<PlantEdge.EdgeType, Gradient>
        {
            { PlantEdge.EdgeType.Root, rootGradient },
            { PlantEdge.EdgeType.Stem, stemGradient },
            { PlantEdge.EdgeType.Wood, woodGradient }
        };
    }
}
