using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapTrailMover : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float moveSpeed = 100f;


    [SerializeField] private ParticleSystem particleSystem;


    // If distance is within this threshold, destroy self
    // private const float MAX_DIST = 0.7f;
    [SerializeField] private float MAX_DIST = 1.5f;
    [SerializeField] private float DECAY_RATE = 0.5f;

    private const float LINGER_TIME = 1.5f;//1.0f;


    void Awake()
    {
        
    }

    // Using FixedUpdate so movement is framerate independent
    void FixedUpdate()
    {
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist >= MAX_DIST)   MoveToTarget();
        else                    DestroySequence();
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
    void DestroySequence()
    { 
        // particleSystem.Play();
        // trailRenderer.emitting = false; // disables trail generation
        trailRenderer.time -= DECAY_RATE;
        Destroy(gameObject, LINGER_TIME);
    }
}
