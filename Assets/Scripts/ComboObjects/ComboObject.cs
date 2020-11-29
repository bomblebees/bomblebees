using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboObject : MonoBehaviour
{
    public float pushedSpeed = 5000f;
    private Vector3 puncherPosition;
    protected virtual void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    protected virtual IEnumerator TickDown()
    {
        yield return new WaitForSeconds(4);
        this.DestroySelf();
    }

    protected virtual void PunchedAction()
    {
        this.Push();
    }
    
    protected virtual void Push()
    {
        var rigidBody = this.GetComponent<Rigidbody>();
        if (!rigidBody)
        {
            Debug.LogError("ComboObject.cs: ComboObject has no RigidBody component.");
        }
        else
        {
            Vector3 distVector = this.gameObject.transform.position - puncherPosition;
            distVector.Normalize();
            Debug.Log(distVector);
            distVector.x *= pushedSpeed; distVector.y = 0; distVector.z *= pushedSpeed;
            rigidBody.velocity = Vector3.zero; rigidBody.angularVelocity = Vector3.zero;  // reset force
            rigidBody.AddForce(distVector);
        }
    }

    private void SetPuncherPosition(Vector3 vec)
    {
        puncherPosition = vec;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Punch"))
        {
            this.SetPuncherPosition(other.gameObject.GetComponent<Transform>().position);
            this.PunchedAction();
        }
    }
}
