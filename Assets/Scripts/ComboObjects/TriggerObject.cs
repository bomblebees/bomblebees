using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// To change
// - Push() to spawn laser
// - TickDown() to not 
public class TriggerObject : ComboObject
{
    [SerializeField] LineRenderer telegraphBeam;
    [SerializeField] GameObject telegraphBall;
    public float breakdownDuration = 3f;
    public bool wasHit = false;
    protected bool canBeTriggered = true; // to make sure it's only hit once.
    protected float timeAlive = 0f; // Seconds since object was instantiated
    protected float timeTriggered = 0f;
    public float minimumLifeDuration = 4f; // Minimum time the object needs to use effect for

    public float rotateLerpRate = 0.15f;
    protected float targetAngle = 0f;
    protected float startAngle;
    protected bool isRotating = false;
	public bool isSpinnable = true;
    public float traumaAmount; //temp


	protected IEnumerator
        procCoroutine =
            null; // when Proc coroutine starts, store to this null var so that we can reference and stop it in Breakdown() even after Proc happens

    private bool canBeExtended = true;
    // note: lingerDuration is the time spent until object despawns without being hit

    public bool stayPermanent = true; // Whether the deployable stays permanently until hit

    public override void OnStartClient()
    {
        base.OnStartClient();
        base.ReadyFillShader();

        //base.OnStartServer();
        FindCenter();
        GoToCenter();
        NotifyOccupiedTile(true);
        ReadyFillShader();
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
        }
    }

    protected virtual void ListenForTrigger()
    {
        // If the coroutine hasn't been started yet, start the Proc coroutine
        if (canBeTriggered && wasHit && procCoroutine == null)
        {
            procCoroutine = Proc();
            StartCoroutine(procCoroutine);
            // "Toggle on" radial timer
            // bombRadialTimerImage.transform.localScale = new Vector3(0.26f,0.26f,0.26f);
            // bombRadialTimerImage.transform.localScale *= 1.25f;
            this.bombRadialTimerImage.color = new Vector4(1,1,1,.75f);          // Enable radial timer
            if (telegraphBeam)
            {
                this.telegraphBeam.gameObject.SetActive(true);   // Enable telegraph beam
                LeanTween.value(telegraphBeam.gameObject, telegraphSizeCallback, 10f, 0.2f, 0.8f); // Size tween
                LeanTween.value(telegraphBeam.gameObject, telegraphGlowCallback, 0.5f, 3f, 0.8f); // Glow tween
            }
            if (telegraphBall)
            {
                this.telegraphBall.SetActive(true);
            }
        }
    }

    void telegraphSizeCallback(float val) { telegraphBeam.SetWidth(val, val); }
    void telegraphGlowCallback(float val) { telegraphBeam.material.SetFloat("_GlowScalar", val); }//telegraphBeam.SetWidth(val, val); }

    protected virtual void StartDangerAnim()
    {
        // TO OVERRIDE IN CHILDREN
    }

    protected virtual IEnumerator Proc()
    {
        if (ownerIsQueen) yield return new WaitForSeconds(queenStartupDelay * fillShaderRatio);
        else yield return new WaitForSeconds(startupDelay * fillShaderRatio);

        StartDangerAnim();

        if (ownerIsQueen) yield return new WaitForSeconds(queenStartupDelay - queenStartupDelay * fillShaderRatio);
        else yield return new WaitForSeconds(startupDelay - startupDelay * fillShaderRatio);

		Debug.Log("proccing triggerobject");
		
        canBeTriggered = false; // To stop it from being triggered twice
        timeTriggered = timeAlive;
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());

		// StartCoroutine(DisableObjectCollider());
        // StartCoroutine(DisableObjectModel());
    }

    protected virtual void ListenForDespawn()
    {
        if (stayPermanent)
        {
            // deployables can not get destroyed until it is hit
            if (canBeExtended && wasHit)
            {
                canBeExtended = false;
                StartCoroutine(DestroySelf(lingerDuration));
            }

            return;
        }

        // deployables will be destroyed even if it is not hit
        if (canBeExtended && timeAlive > lingerDuration)
        {
            canBeExtended = false;
            if (wasHit)
            {
                if (lingerDuration - timeTriggered < minimumLifeDuration + startupDelay
                ) // If hit and going into "overtime" with trigger
                {
                    float extensionTime = minimumLifeDuration + startupDelay - (lingerDuration - timeTriggered);
                    StartCoroutine(DestroySelf(extensionTime));
                }
                else // If was hit and not going into "overtime" with trigger
                {
                    StartCoroutine(DestroySelf());
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
        // Play shake anim
        FindObjectOfType<CameraShake>().InduceStress(traumaAmount);
        
        var hitbox = this.gameObject.transform.Find("Hitbox").gameObject;

		// once hitbox is enabled, triggerobject cannot be spun anymore
		isSpinnable = false;
		hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxDuration);
        hitbox.SetActive(false);
		StartCoroutine(Breakdown());
    }

    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        // NotifyOccupiedTile(false); // prolly move this later
        wasHit = true;
        return true;
    }

    public virtual IEnumerator Breakdown()
    {
        didEarlyEffects = true;
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());

		hitBox.SetActive(false);

        NotifyOccupiedTile(false);

        // if the laser has already been procced when the breakdown happens, stop the proc prematurely before destroying
        if (procCoroutine != null)
        {
            Debug.Log("Stopping Proc coroutine");
            StopCoroutine(procCoroutine);
        }

        // play breakdown anim here
        yield return new WaitForSeconds(breakdownDuration);

        DestroySelf();
    }

    protected virtual void GetSpunDirection(int edgeIndex, GameObject triggeringPlayer, bool useBeam)
    {
        /* Note from Terence:
            I commented this out to fix the rotation bug. The rotation is handled
            by 'targetAngle = HexMetrics.edgeAngles[edgeIndex] -90f' instead.
            This seems to work more consistently and the extra angle math seems
            redundant since we pass in edgeIndex anyway? But talk to me if this
            breaks something bc I might b dumdum too
        */

        // get angle from player
        // Vector3 dir = triggeringPlayer.transform.position - transform.position;
        // dir = triggeringPlayer.transform.InverseTransformDirection(dir);

        // float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        // angle += 90f;
        // targetAngle = RoundAngleToHex(angle);

        targetAngle = HexMetrics.edgeAngles[edgeIndex] -90f;



        // Smooth rotate, try without?
        startAngle = model.transform.eulerAngles.y;
        if (Math.Abs(startAngle - targetAngle) >= 180) // if angle diff is > 180, rotate in opp direction
        {
            if (startAngle >= targetAngle) targetAngle += 360f;  // wraparound
            // startAngle += 360f;  // reset
        }

        // TODO: Take another look at this, the model spin is bugging at a certain angle
        // Debug.Log("Start angle: "+startAngle+", Target angle: "+targetAngle);

        isRotating = true;
        if (useBeam)
        {
            this.gameObject.GetComponent<Transform>().transform.Find("Hitbox").transform.eulerAngles =
                new Vector3(90f, 0f, -HexMetrics.edgeAngles[edgeIndex]);
        }

        // this.gameObject.GetComponent<Transform>().transform.Find("VFX").transform.eulerAngles =
        //     new Vector3(-90f, 0f, HexMetrics.edgeAngles[edgeIndex] + 270f);
    }
}