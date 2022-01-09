using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeUpgrader : Singleton<EdgeUpgrader>
{
    [Header("References")]
    [SerializeField]
    private TemporaryEdgeController temporaryEdge;

    private bool retryCollisionCheck = false;
    private PlantEdge edgeToTest;
    UpgradedEdgeCollision currentEdgeCollisionStatus;
    private bool retryUpgrade = false;
    private PlantEdge edgeToUpgrade;

    enum UpgradedEdgeCollision
    {
        Collision,
        NoCollision,
        DontKnow
    }

    private void Update()
    {
        if (retryCollisionCheck || retryUpgrade)
        {
            currentEdgeCollisionStatus = CheckIfCollides(edgeToTest);
            SetCollisionGraphics(currentEdgeCollisionStatus);
            if (retryUpgrade)
            {
                Upgrade(edgeToUpgrade);
            }
        }
    }

    public void TestUpgrade(PlantEdge edge)
    {
        if (edge.Level >= PlantConfigManager.Instance.edgeWidthsOnLevels.Count - 1)
        {
            return;
        }
        if (edge == edgeToTest && currentEdgeCollisionStatus != UpgradedEdgeCollision.DontKnow)
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

    public void Upgrade(PlantEdge edge)
    {
        if (edge.Level >= PlantConfigManager.Instance.edgeWidthsOnLevels.Count - 1)
        {
            Debug.Log("Max edge upgrade level reached");
            return;
        }
        edgeToUpgrade = edge;
        if (edgeToTest == edgeToUpgrade)
        {
            switch (currentEdgeCollisionStatus)
            {
                case UpgradedEdgeCollision.NoCollision:
                    edgeToUpgrade.Level += 1;
                    retryUpgrade = false;
                    currentEdgeCollisionStatus = UpgradedEdgeCollision.DontKnow;
                    break;
                case UpgradedEdgeCollision.Collision:
                    Debug.Log("Edge collides with something");
                    retryUpgrade = false;
                    return;
                case UpgradedEdgeCollision.DontKnow:
                    edgeToTest = edgeToUpgrade;
                    retryUpgrade = true;
                    break;
                default:
                    break;
            }
        }
        else
        {
            edgeToTest = edgeToUpgrade;
            retryUpgrade = true;
        }
    }

    private void SetCollisionGraphics(UpgradedEdgeCollision collisionStatus)
    {
        Debug.Log(collisionStatus);
        switch (collisionStatus)
        {
            case UpgradedEdgeCollision.Collision:
                temporaryEdge.PlacementCorrectness = false;
                retryCollisionCheck = false;
                break;
            case UpgradedEdgeCollision.NoCollision:
                temporaryEdge.PlacementCorrectness = true;
                retryCollisionCheck = false;
                break;
            case UpgradedEdgeCollision.DontKnow:
                retryCollisionCheck = true;
                break;
            default:
                Debug.Log("Edge update error - this should never happen");
                break;
        }
    }

    public void HideTemporaryEdge()
    {
        temporaryEdge.gameObject.SetActive(false);
        retryCollisionCheck = false;
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
            if (collidingEdge.end == edgeToUpgrade.begin)
            {
                collides = false;
            }
            if (collidingEdge.begin == edgeToUpgrade.end)
            {
                collides = false;
            }
            if (collidingEdge == edgeToUpgrade)
            {
                collides = false;
            }
            if (collides)
            {
                return UpgradedEdgeCollision.Collision;
            }
        }
        return UpgradedEdgeCollision.NoCollision;
    }
}
