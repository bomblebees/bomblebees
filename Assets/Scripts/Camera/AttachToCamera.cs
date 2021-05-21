using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachToCamera : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        cam = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
            
        if (canvas != null)
        {
            canvas.worldCamera = cam;
        }

    }
}
