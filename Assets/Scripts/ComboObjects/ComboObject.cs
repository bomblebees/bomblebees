using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboObject : MonoBehaviour
{
    public float pushedSpeed = 5000f;
    private Vector3 puncherPosition;
    private Vector3 tileCoordUnderPuncher;
    private float pushedDir = 30;

    protected virtual void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    protected virtual IEnumerator TickDown()
    {
        yield return new WaitForSeconds(4);
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(DisableObjectModel());
        StartCoroutine(EnableHitbox());
        yield return new WaitForSeconds(2);
        this.DestroySelf();
    }
    
    protected virtual IEnumerator EnableSFX()
    {
        yield return new WaitForSeconds(0);
    }

    protected virtual IEnumerator EnableVFX()
    {
        var animation = this.gameObject.transform.Find("Animation");
        animation.gameObject.SetActive(true);
        yield return new WaitForSeconds(0);
    }
    
    protected virtual IEnumerator DisableObjectModel()
    {
        var model = this.gameObject.transform.Find("Model");
        model.gameObject.SetActive(false);
        yield return new WaitForSeconds(0);
    }

    protected virtual IEnumerator EnableHitbox()
    {
        var hitbox = this.gameObject.transform.Find("Hitbox");
        hitbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(0);
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
}