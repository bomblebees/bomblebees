using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapTrailMover : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 100f;


    // If distance is within this threshold, destroy self
    private const float MAX_DIST = 0.05f;
    private const float LINGER_TIME = 0.6f;//1.0f;


    // Using FixedUpdate so movement is framerate independent
    void FixedUpdate()
    {
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist >= MAX_DIST)   MoveToTarget();
        else                    DestroySelf();
    }

    // <summary>
    // Moves object to the target destination
    // </summary>
    void MoveToTarget()
    {
        float stepDistance = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position,
                                                    target.position,
                                                       stepDistance);
    }

    // <summary>
    // Updates targetPos to new target
    // </summary>
    public void AssignTarget(Transform newTarget) { target = newTarget; }


    // <summary>
    // Destroys the object
    // </summary>
    void DestroySelf() { Destroy(gameObject, LINGER_TIME); }
}
