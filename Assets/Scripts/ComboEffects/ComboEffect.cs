using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboEffect : MonoBehaviour
{
    protected float tickDuration = 4;
    protected virtual void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    protected virtual IEnumerator TickDown()
    {
        yield return new WaitForSeconds(tickDuration);
        this.DestroySelf();
    }
}
