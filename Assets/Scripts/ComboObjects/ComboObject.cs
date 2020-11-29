using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboObject : MonoBehaviour
{
    protected virtual void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    protected virtual IEnumerator TickDown()
    {
        yield return new WaitForSeconds(4);
        this.DestroySelf();
    }
}
