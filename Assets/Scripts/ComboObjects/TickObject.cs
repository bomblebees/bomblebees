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
        this.bombRadialTimerImage.color = new Vector4(1,1,1,.75f);
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
        ProcEffects();
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

    public void ProcEffects()
    {
        wasProcced = true;
        LeanTween.cancel(moveTweenId);

        // yield return new WaitForSeconds(0.01f);
        StopVelocity();
        StartCoroutine(EnableSFX());
        StartCoroutine(EnableVFX());
        StartCoroutine(EnableHitbox());
        StartCoroutine(DisableObjectCollider());
        StartCoroutine(DisableObjectModel());
        NotifyOccupiedTile(false);
    }


    public virtual void EarlyProc(GameObject proccingBomb)
    {

        if (wasProcced) return; // Once procced, cannot proc again
        //Debug.Log("is this bomb moving? - " + isMoving);

        wasMovingWhenProcced = isMoving;

        ComboObject parentBomb = proccingBomb.GetComponent<ComboObject>();


        if (parentBomb.wasMovingWhenProcced) 
        {
            // if parent bomb was moving, then proccer is the parent bomb's triggering player
            // regardless of if the parent has a proccing player or not (this is the new proccer)

            this.proccingPlayerBefore = parentBomb.triggeringPlayerBefore;
            this.proccingPlayer = parentBomb.triggeringPlayer;

            Debug.Log("EarlyProc - parent was moving bomb");
        }
        else if (parentBomb.proccingPlayer != null) // If parent bomb has proc
        {
            // Inherit the proccing player from the parent
            this.proccingPlayerBefore = parentBomb.proccingPlayerBefore;
            this.proccingPlayer = parentBomb.proccingPlayer;

            Debug.Log("EarlyProc - inherited proccing player: " + proccingPlayer.GetComponent<Player>().playerRoomIndex);
        }
        else // if parent bomb was not moving and had no proc, then the parent bomb is the proccer
        {
            // The trigger player of the parent (root) bomb is the proccing player
            this.proccingPlayerBefore = parentBomb.triggeringPlayerBefore;
            this.proccingPlayer = parentBomb.triggeringPlayer;

            Debug.Log("EarlyProc - set proccing player: " + proccingPlayer.GetComponent<Player>().playerRoomIndex);
        }

        if (isLocalPlayer) FindObjectOfType<AudioManager>().StopPlaying("bombBeep");
        if (didEarlyEffects) return;
        didEarlyEffects = true;
        FindCenter();
        GoToCenter();
        ProcEffects();
    }

	protected override void OnTriggerEnter(Collider other)
	{
		base.OnTriggerEnter(other);
		if (other.gameObject.CompareTag("Spin"))
		{
			NotifyOccupiedTile(false);
		}
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


    /// <summary>
    /// Returns the player who is responsible for any kills that this bomb acquires
    /// </summary>
    public override GameObject GetKillerPlayer(GameObject playerThatWasKilled)
    {

        // If regular bomb kill, just get last interactor
        if (!didEarlyEffects)
        {
            Debug.Log("regular bomb kill");
            return triggeringPlayer.Equals(playerThatWasKilled) ? triggeringPlayerBefore : triggeringPlayer;
        }

        // If bomb was moving, award the kill to the triggering player of this bomb
        if (wasMovingWhenProcced && triggeringPlayer != playerThatWasKilled)
        {
            Debug.Log("moving bomb kill");
            return triggeringPlayer.Equals(playerThatWasKilled) ? triggeringPlayerBefore : triggeringPlayer;
        }

        Debug.Log("proc bias kill");

        // else the killer must be the proccer
        return proccingPlayer.Equals(playerThatWasKilled) ? proccingPlayerBefore : proccingPlayer;
    }

}