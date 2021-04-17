using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// To change
// - Push() to spawn laser
// - TickDown() to not 
public class LaserObject : TriggerObject
{
    public float breakdownDuration = 3f;
    private bool isRotating = false;
    private float targetAngle = 0f;
    public float rotateLerpRate = 0.15f;
    
    protected override void StartDangerAnim()
    {
        this.model.GetComponent<Renderer>().materials[0].SetFloat("_WobbleToggle", 1f);
        this.model.GetComponent<Renderer>().materials[1].SetFloat("_WobbleToggle", 1f);
    }
    
    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        FindObjectOfType<AudioManager>().PlaySound("laserCharge");
        UpdateLaserDirection(edgeIndex, triggeringPlayer);
        return base.Push(edgeIndex, triggeringPlayer);  // Uses TriggerObject.Push(). If a bug arises, switch order
    }

    protected virtual void UpdateLaserDirection(int edgeIndex, GameObject triggeringPlayer)
    {
        // get angle from player
        Vector3 dir = triggeringPlayer.transform.position - transform.position;
        dir = triggeringPlayer.transform.InverseTransformDirection(dir);
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        // angle = Mathf.Rad2Deg(angle);
        angle += 90f+360f;
        float[] roundTo = new float[]{-60f, 0f, 60f, 120f, 180f, 240f, 300f};
        targetAngle = RoundAngleToHex(angle);
        
        // model.transform.eulerAngles = new Vector3(0f, angle, 0f);
        // targetAngle = angle;
        isRotating = true;
        this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles = new Vector3(90f, 0f, -HexMetrics.edgeAngles[edgeIndex]);
        this.gameObject.GetComponent<Transform>().transform.Find("VFX").transform.eulerAngles = new Vector3(-90f, 0f, HexMetrics.edgeAngles[edgeIndex]+270f);
    }

    protected virtual float RoundAngleToHex(float angle)
    {

        bool neg = angle < 0 ? true : false;
        if (neg) angle += 360f;
        float remainder = Math.Abs(angle) % 60;
        float newAngle = angle;
        if (remainder < 30f)  // round down
        {
            newAngle -= remainder;
        }
        else
        {
            newAngle += (60 - remainder);
        }

        if (neg) newAngle -= 360f;
        print("angle "+angle+" going to "+newAngle);
        return newAngle;
    }
    
    // Note: this is when THIS object enters a collision
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var gameObjHit = other.gameObject;
        if (gameObjHit.CompareTag("ComboHitbox"))
        {
            var _root = gameObjHit.transform.root.name;
            if (_root.Equals("Bomb Object(Clone)"))
            {
                StartCoroutine(Breakdown());
            }
        }
    }
    
    protected virtual void Update()
    {
        if (hasStarted == true)
        {
            ListenForTrigger();
            ElapseTimeAlive();
            ListenForDespawn();
            if (wasHit)
            {
                StepFillShader();
            }

            if (isRotating)
            {
                var r = model.transform.rotation.eulerAngles;
                print(r.y+" GOING TO "+targetAngle);
                this.model.transform.eulerAngles = Vector3.Lerp(new Vector3(r.x, r.y+360f, r.z), 
                        new Vector3(0f, targetAngle, 0f), rotateLerpRate); // move object               
            }
        }
    }
    
    public IEnumerator Breakdown()
    {
        didEarlyEffects = true;
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        // play breakdown anim here
        NotifyOccupiedTile(false);
        yield return new WaitForSeconds(breakdownDuration);
        DestroySelf();
    }
}