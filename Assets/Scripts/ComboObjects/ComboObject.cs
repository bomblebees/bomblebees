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
        this.DestroySelf();
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

    // Deprecated
    protected Vector3 GetTileCoordUnderPuncher(Collider other)
    {
        var playerRay = new Ray(other.gameObject.transform.position, Vector3.down);
        RaycastHit tileUnderneath;


        // old method

        if (Physics.Raycast(playerRay, out tileUnderneath, 1000f, 1 << LayerMask.NameToLayer("BaseTiles")))
        {
            // make a sphere at point
            var result = tileUnderneath.transform.position; // this is right
            Instantiate(Resources.Load("Prefabs/Debug/Cylinder"), result, Quaternion.identity);
            return result;
        }
        else
            // remove this
            return Vector3.down;
    }
}