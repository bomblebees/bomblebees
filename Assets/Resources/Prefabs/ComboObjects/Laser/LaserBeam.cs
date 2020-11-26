using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    private float lifeDuration = 0.5f;
    private float sizeScalar = 2f;

    // Start is called before the first frame update
    private void Rotate(HexDirection dir)
    {
        Vector3 newDirVec;
        // set transforms rotation here.
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
            case HexDirection.NE:
                newDirVec = new Vector3(90f, 0f, 150f);
                break;
            default:
                newDirVec = new Vector3(90f, 0f, 150f);
                break;
        }
        if (dir == HexDirection.NW)
            
        this.gameObject.transform.Rotate(newDirVec); 
    }

    public void SpawnInDirection(HexDirection direction)
    {
        Rotate(direction);
        StartCoroutine(TickDown());
    }

    IEnumerator TickDown()
    {
        yield return new WaitForSeconds(lifeDuration);
        DestroySelf();
    }

    private void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(other.gameObject);
        }
    }
}