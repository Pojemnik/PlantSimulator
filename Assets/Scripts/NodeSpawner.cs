using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSpawner : MonoBehaviour
{
    [SerializeField]
    private PlantNode plantCore;

    [Header("Plant base config")]
    [Header("Stem node")]
    [SerializeField]
    private Vector2 stemTop;
    [Header("Root node")]
    [SerializeField]
    private Vector2 rootBottom;

    private void Start()
    {
        CreatePlantBase();
    }

    private void CreatePlantBase()
    {

    }
}

