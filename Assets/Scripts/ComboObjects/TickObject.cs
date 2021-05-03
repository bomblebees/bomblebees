using System.Collections;
using UnityEngine;

public class TickObject : ComboObject
{

    private bool tickStarted = false;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        // All the client needs to do is render the sfx/vfx
        // The server should do all calculations
    }

    public virtual void _Start(GameObject player)
    {
        base._Start(player);
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        FindCenter();
        GoToCenter();
        NotifyOccupiedTile(true);
    }
    
    protected override void Update()
    {
        base.Update();
        base.StepFillShader();
    }
    
    protected virtual IEnumerator TickDown()
    {
        tickStarted = true;
        base.EnableBeepSFX();
        base.ReadyFillShader();
        // "Toggle on" radial timer
            // bombRadialTimerImage.transform.localScale = new Vector3(0.26f,0.26f,0.26f);
            // bombRadialTimerImage.transform.localScale *= 1.25f;
        this.bombRadialTimerImage.color = new Vector4(1,1,1,1);
        if (!didEarlyEffects)
        {
            if (ownerIsQueen)
            {
                yield return new WaitForSeconds(fillShaderRatio * queenStartupDelay);
            }
            else
            {
                yield return new WaitForSeconds(fillShaderRatio * startupDelay);
            }
            StartDangerAnim();
        }

        if (ownerIsQueen)
        {
            yield return new WaitForSeconds(queenStartupDelay - fillShaderRatio * queenStartupDelay);
        }
        else
        {
            yield return new WaitForSeconds(startupDelay - fillShaderRatio * startupDelay);
        }
        if (!didEarlyEffects)
        {
            StartCoroutine(TickDownFinish());
        }
    }

    protected virtual IEnumerator TickDownFinish()
    {
        FindCenter();
        GoToCenter();
        StartCoroutine((ProcEffects()));
        if (!didEarlyEffects)
        {
            yield return new WaitForSeconds(lingerDuration);
            StartCoroutine(DestroySelf());
        }
    }
    
    protected virtual void StartDangerAnim()
    {
        // TO DEFINE IN EACH BOMB TYPE
    }

    public virtual IEnumerator ProcEffects()
    {
        print("Proccing");
        yield return new WaitForSeconds(0.01f);
        StopVelocity();
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        NotifyOccupiedTile(false);
    }


    public virtual void EarlyProc()
    {
        if (isLocalPlayer) FindObjectOfType<AudioManager>().StopPlaying("bombBeep"); // TODO do something like "StopPlaying(objectSound)"
        didEarlyEffects = true;
        FindCenter();
        GoToCenter();
        StartCoroutine(ProcEffects());
    }


    protected override bool Push(int edgeIndex, GameObject triggeringPlayer)
    {
        if (!tickStarted) StartCoroutine(TickDown());
        bool result = base.Push(edgeIndex, triggeringPlayer);
        if (result)
        {
            NotifyOccupiedTile(false);
        }
        this.blockerHandler.SetActive(false); // in order to stop blocking players while moving
        return result;
    }
    
}