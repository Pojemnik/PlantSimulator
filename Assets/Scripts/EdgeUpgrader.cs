using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeUpgrader : Singleton<EdgeUpgrader>
{
    [Header("References")]
    [SerializeField]
    private TemporaryEdgeController temporaryEdge;

    private bool retry = false;
    PlantEdge edgeToTest;

    enum UpgradedEdgeCollision
    {
        Collision,
        NoCollision,
        DontKnow
    }

    private void Update()
    {
        if (retry)
        {
            UpgradedEdgeCollision collisionStatus = CheckIfCollides(edgeToTest);
            SetCollisionGraphics(collisionStatus);
        }
    }

    public void TestUpgrade(PlantEdge edge)
    {
        if (edge.Level >= PlantConfigManager.Instance.edgeWidthsOnLevels.Count)
        {
            return;
        }
        edgeToTest = edge;
        temporaryEdge.gameObject.SetActive(true);
        temporaryEdge.Level = edge.Level + 1;
        temporaryEdge.edge.begin = edge.begin;
        temporaryEdge.edge.end = edge.end;
        temporaryEdge.UpdateEdgePosition();
        UpgradedEdgeCollision collisionStatus = CheckIfCollides(edge);
        SetCollisionGraphics(collisionStatus);
    }

    private void SetCollisionGraphics(UpgradedEdgeCollision collisionStatus)
    {
        Debug.Log(collisionStatus);
        switch (collisionStatus)
        {
            case UpgradedEdgeCollision.Collision:
                temporaryEdge.PlacementCorrectness = false;
                retry = false;
                break;
            case UpgradedEdgeCollision.NoCollision:
                temporaryEdge.PlacementCorrectness = true;
                retry = false;
                break;
            case UpgradedEdgeCollision.DontKnow:
                retry = true;
                break;
            default:
                Debug.Log("Edge update error - this should never happen");
                break;
        }
    }

    public void HideTemporaryEdge()
    {
        temporaryEdge.gameObject.SetActive(false);
        retry = false;
    }

    private UpgradedEdgeCollision CheckIfCollides(PlantEdge edgeToUpgrade)
    {
        List<Collider2D> contacts = new List<Collider2D>();
        temporaryEdge.collider.GetContacts(contacts);
        if (contacts.Count == 0)
        {
            return UpgradedEdgeCollision.DontKnow;
        }
        foreach (Collider2D collider in contacts)
        {
            PlantEdge collidingEdge = collider.gameObject.GetComponent<PlantEdge>();
            bool collides = true;
            if (collidingEdge.begin == edgeToUpgrade.begin)
            {
                collides = false;
            }
            if(collidingEdge.end == edgeToUpgrade.begin)
            {
                collides = false;
            }    
            if(collidingEdge.begin == edgeToUpgrade.end)
            {
                collides = false;
            }
            if(collidingEdge == edgeToUpgrade)
            {
                collides = false;
            }
            if(collides)
            {
                return UpgradedEdgeCollision.Collision;
            }
        }
        return UpgradedEdgeCollision.NoCollision;
    }
}
