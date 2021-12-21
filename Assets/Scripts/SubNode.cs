using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SubNode : MonoBehaviour, IInteractive
{
    public void OnInteraction(Vector2 position)
    {
        Debug.LogFormat("Interaction with SubNode {0} on position {1}", gameObject.name, position);
    }
}
