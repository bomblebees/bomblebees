using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Required Children:
// - VFX
// - SFX
// - Hitbox
public class ComboObject : MonoBehaviour
{
    public float pushedSpeed = 5000f;
    private Vector3 puncherPosition;
    private Vector3 tileCoordUnderPuncher;
    private float pushedDir = 30;
    
    public float tickDuration = 4f;
    public float vfxDuration = 4f;
    public float sfxDuration = 4f;
    public float hitboxDuration = 4f;
    public float lifeDuration = 8f;   
    public bool useUniversalVal = false; // Replaces the other durations (except lifeDuration)
    public float universalVal = 4f;
     
    protected virtual IEnumerator TickDown()
    {
        yield return new WaitForSeconds(tickDuration);
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());
        StartCoroutine(DisableObjectModel());
        yield return new WaitForSeconds(lifeDuration);
        DestroySelf();
    }
    
    protected virtual IEnumerator EnableVFX()
    {
        this.gameObject.transform.Find("VFX").gameObject.SetActive(true);
        if (!useUniversalVal) yield return new WaitForSeconds(vfxDuration);
        else yield return new WaitForSeconds(universalVal);
    }
    
    protected virtual IEnumerator EnableSFX()
    {
        this.gameObject.transform.Find("SFX").gameObject.SetActive(true);
        if (!useUniversalVal) yield return new WaitForSeconds(sfxDuration);
        else yield return new WaitForSeconds(universalVal);
    }
    
    protected virtual IEnumerator EnableHitbox()
    {
        this.gameObject.transform.Find("Hitbox").gameObject.SetActive(true);
        if (!useUniversalVal) yield return new WaitForSeconds(hitboxDuration);
        else yield return new WaitForSeconds(universalVal);
    }
    
    protected virtual IEnumerator DisableObjectModel()
    {
        this.gameObject.transform.Find("Model").gameObject.SetActive(true);
        yield return new WaitForSeconds(lifeDuration);
    }

    protected virtual void PunchedAction(int edgeIndex)
    {
        this.Push(edgeIndex);
    }

    protected virtual void Push(int edgeIndex)
    {
        var rigidBody = this.GetComponent<Rigidbody>();
        if (!rigidBody)
        {
            Debug.LogError("ComboObject.cs: ComboObject has no RigidBody component.");
        }
        else
        {
            rigidBody.AddForce(HexMetrics.edgeDirections[edgeIndex] * pushedSpeed);
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Spin"))
        {
            var playerPosition = other.transform.parent.gameObject.transform.position;
            // note: Don't use Vector3.Angle
            var angleInRad = Mathf.Atan2(playerPosition.x - transform.position.x, playerPosition.z - transform.position.z);
            
            var dirFromPlayerToThis = (Mathf.Rad2Deg * angleInRad) + 180;  // "+ 180" because Unity ranges it from [-180, 180]

            pushedDir = 30f;
            int edgeIndex;
            for (edgeIndex = 0; edgeIndex < HexMetrics.edgeAngles.Length; edgeIndex++)
            {
                Debug.Log(dirFromPlayerToThis + " - " + HexMetrics.edgeAngles[edgeIndex]);
                if (Mathf.Abs(dirFromPlayerToThis - HexMetrics.edgeAngles[edgeIndex]) <= 30)
                {
                    pushedDir = HexMetrics.edgeAngles[edgeIndex];
                    break;
                }
            }
            Debug.Log("target dir is " + pushedDir);
            this.PunchedAction(edgeIndex);
        }
    }
    
    protected virtual void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}