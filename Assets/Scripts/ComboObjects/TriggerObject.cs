using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter.Xml;
using UnityEngine;

// To change
// - Push() to spawn laser
// - TickDown() to not 
public class TriggerObject : ComboObject
{
    protected bool wasHit = false;
    protected bool canBeTriggered = true; // to make sure it's only hit once.
    protected float timeAlive = 0f; // Seconds since object was instantiated
    protected float timeTriggered = 0f;
    protected float minimumLifeDuration = 4f; // Minimum time the object needs to use effect for

    private bool canBeExtended = true;
    // note: lingerDuration is the time spent until object despawns without being hit
    
    protected virtual void Start()
    {
        FindCenter();
        GoToCenter();
        NotifyOccupiedTile(true);
    }

    protected override IEnumerator TickDown()
    {
        yield return new WaitForSeconds(0);
    }

    protected override void Update()
    {
        ListenForTrigger();
        ElapseTimeAlive();
        ListenForDespawn();
    }

    protected virtual void ListenForTrigger()
    {
        if (canBeTriggered && wasHit)
        {
            Debug.Log("triggered");
            canBeTriggered = false;  // To stop it from being triggered twice
            timeTriggered = timeAlive;
            StartCoroutine(EnableSFX());
            StartCoroutine(EnableVFX());
            StartCoroutine(EnableHitbox());
            StartCoroutine(DisableObjectCollider());
            StartCoroutine(DisableObjectModel());
        }
    }

    protected virtual void ListenForDespawn()
    {
        if (canBeExtended && timeAlive > lingerDuration)
        {
            canBeExtended = false;
            if (wasHit)
            {
                if (lingerDuration - timeTriggered < minimumLifeDuration) // extend here
                {
                    float extensionTime = minimumLifeDuration - (lingerDuration - timeTriggered);
                    StartCoroutine(DestroySelf(extensionTime));
                }
            }
            else
            {
                StartCoroutine(DestroySelf());
            }
        }
    }

    protected virtual void ElapseTimeAlive()
    {
        timeAlive += Time.deltaTime;
    }

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
    }
}