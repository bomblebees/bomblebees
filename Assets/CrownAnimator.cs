using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownAnimator : MonoBehaviour
{
    // <summary>
    // Activates crown object and applies bounce animation
    // </summary>
    public void EnableCrown()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.one;
        GetComponent<ScaleTween>().StartTween();
    }

    // <summary>
    // Disables crown object
    // </summary>
    public void DisableCrown()
    {
        gameObject.SetActive(false);
    }


}
