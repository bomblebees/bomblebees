using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter.Xml;
using UnityEngine;

public class LaserObject : MonoBehaviour
{
    private float tickDuration = 2f;
    public HexDirection hexDirection;  
    
    /* SetDirection:
        Called (separately) when LaserObject is Instantiated.
    */ 
    public void SetDirection(HexDirection dir)
    {
        this.hexDirection = dir;
    }

    private void Awake()
    {
        StartCoroutine(TickDown());
    }

    IEnumerator TickDown()
    {
        Debug.Log(String.Concat("LaserObject is ticking for ", tickDuration));
        yield return new WaitForSeconds(tickDuration);
        this.Lase(hexDirection);
        this.DestroySelf();
    }

    private void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    private void Lase(HexDirection hexDirection)
    {
        Debug.Log("Creating Bomb Explosion");
        var laser = Instantiate(Resources.Load("Prefabs/ComboObjects/Laser/Laser Beam"),
            new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z),
            Quaternion.identity) as GameObject;
        if (!laser) Debug.Log("LaserObject.cs: could not instantiate laser beam.");
        else
        {
            laser.GetComponent<LaserBeam>().SpawnInDirection(hexDirection);
        }
    }
}