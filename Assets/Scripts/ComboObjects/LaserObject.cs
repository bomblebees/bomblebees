using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter.Xml;
using UnityEngine;

// To change
// - Push() to spawn laser
// - TickDown() to not 
public class LaserObject : TriggerObject
{
    
    protected override IEnumerator EnableHitbox()
    {
        // 
        this.gameObject.transform.Find("Hitbox").gameObject.SetActive(true);
        if (!useUniversalVal) yield return new WaitForSeconds(hitboxDuration);
        else yield return new WaitForSeconds(universalVal);
    }

    public Quaternion GetRotationFromDirection(HexDirection dir)
    {
        Vector3 newDirVec;
        switch (dir)
        {
            case HexDirection.NW:
                newDirVec = new Vector3(90f, 0f, 30f);
                break;
            case HexDirection.SE:
                newDirVec = new Vector3(90f, 0f, 30f);
                break;
            case HexDirection.W:
                newDirVec = new Vector3(90f, 0f, 90f);
                break;
            case HexDirection.E:
                newDirVec = new Vector3(90f, 0f, 90f);
                break;
            case HexDirection.SW:
                newDirVec = new Vector3(90f, 0f, 150f);
                break;
            case HexDirection.NE:
                newDirVec = new Vector3(90f, 0f, 150f);
                break;
            default:
                Debug.LogError("LaserObject.cs: This should not be happening.");
                newDirVec = new Vector3(90f, 0f, 150f);
                break;
        }

        return Quaternion.Euler(newDirVec);
    }

    protected override void Push(int edgeIndex)
    {
        wasHit = true;
        UpdateLaserDirection(edgeIndex);
    }

    protected virtual void UpdateLaserDirection(int edgeIndex)
    {
        Debug.Log("edgeIndex is "+edgeIndex);
        this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles = new Vector3(90f, 0f, -HexMetrics.edgeAngles[edgeIndex]);
    }
}