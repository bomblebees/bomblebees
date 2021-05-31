using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownAnimator : MonoBehaviour
{
    // public Vector3 currLocalScale;

    // <summary>
    // Activates crown object and applies bounce animation
    // </summary>
    public void EnableCrown()
    {
        gameObject.SetActive(true);
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
