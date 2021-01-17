using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter.Xml;
using UnityEngine;

public class LaserObject : ComboObject
{
    private HexDirection hexDirection;
    public string effectPath = "Prefabs/ComboEffects/Laser Beam";

    /* SetDirection:
        Called (separately) when LaserObject is Instantiated.
    */
    public void SetDirection(HexDirection dir)
    {
        this.hexDirection = dir;
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

    private void Awake()
    {
    }

    private void Start()
    {
        StartCoroutine(TickDown());
    }

    protected override IEnumerator TickDown()
    {
        Debug.Log(String.Concat("LaserObject is ticking for ", tickDuration));
        yield return new WaitForSeconds(tickDuration);
        this.Lase(hexDirection);
        this.DestroySelf();
    }

    protected override void DestroySelf() 
    {
        Destroy(this.gameObject);
    }

    private void Lase(HexDirection hexDirection)
    {
        Debug.Log("Creating Bomb Explosion");
        var laser = Instantiate(Resources.Load(effectPath),
            new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z),
            GetRotationFromDirection(hexDirection)
            ) as GameObject;
        if (!laser) Debug.Log("LaserObject.cs: could not instantiate laser beam.");
        else
        {
            laser.GetComponent<LaserBeam>().SpawnInDirection(hexDirection);
        }
    }
}