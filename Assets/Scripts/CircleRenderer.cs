using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircleRenderer : MonoBehaviour
{
    [Header("Config")]
    [Range(6, 100)]
    public int points;
    public Color color;
    public float radius;
    public Vector2 position;

    private LineRenderer line;
    private Gradient gradient;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        GenerateAndSetGradient();
        Calculate();
    }

    private void GenerateAndSetGradient()
    {
        if (gradient == null)
        {
            gradient = new Gradient();
        }
        gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(color, 0), new GradientColorKey(color, 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
        line.colorGradient = gradient;
    }

    private void Calculate()
    {
        line.positionCount = points + 1;
        Vector3[] positions = new Vector3[points + 1];
        float z = transform.position.z;
        float step = Mathf.PI * 2 / points;
        for (int i = 0; i < points + 1; i++)
        {
            positions[i] = new Vector3(Mathf.Sin(i * step), Mathf.Cos(i * step), z) * radius + (Vector3)position + transform.position;
        }
        line.SetPositions(positions);
    }
}