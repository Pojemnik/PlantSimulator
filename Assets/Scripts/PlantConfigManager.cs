using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantConfigManager : Singleton<PlantConfigManager>
{
    [SerializeField]
    private Gradient stemGradient;
    [SerializeField]
    private Gradient rootGradient;
    [SerializeField]
    private Gradient woodGradient;
    [SerializeField]
    private int defaultStemWidthIndex;
    [SerializeField]
    private int defaultRootWidthIndex;
    [SerializeField]
    private int defaultWoodWidthIndex;

    public Dictionary<PlantEdge.EdgeType, Gradient> gradients;
    public List<float> edgeWidthsOnLevels;
    public Dictionary<PlantEdge.EdgeType, int> defaultEgdeWidths;
    public float minStemHeight;
    public float maxRootHeight;

    private int nodeCounter = 0;
    public int NodeCounter
    {
        get
        {
            return nodeCounter++;
        }
    }
    
    private int edgeCounter = 0;
    public int EdgeCounter
    {
        get
        {
            return edgeCounter++;
        }
    }

    private void Awake()
    {
        gradients = new Dictionary<PlantEdge.EdgeType, Gradient>
        {
            { PlantEdge.EdgeType.Root, rootGradient },
            { PlantEdge.EdgeType.Stem, stemGradient },
            { PlantEdge.EdgeType.Wood, woodGradient }
        };

        defaultEgdeWidths = new Dictionary<PlantEdge.EdgeType, int>
        {
            { PlantEdge.EdgeType.Root, defaultRootWidthIndex},
            { PlantEdge.EdgeType.Stem, defaultStemWidthIndex},
            { PlantEdge.EdgeType.Wood, defaultWoodWidthIndex}
        };
    }
}
