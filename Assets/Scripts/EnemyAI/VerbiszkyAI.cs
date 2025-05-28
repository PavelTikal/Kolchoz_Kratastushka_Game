using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;

public class VerbiszkyAI : MonoBehaviour
{
    [Header("AI Movement")]
    public float speed = 5f;
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private NavMeshAgent agent;

    [Header("AI Following Player")]
    public Transform player;
    public float detectionRange = 10f;
    public float fieldOfView = 45f;

    private bool isPlayerDetected = false;
    private bool isHit = false;
    private Renderer renderer;
    private AudioSource audioSource;

    [Header("AI Sounds")]
    public AudioClip detectionSound;
    public AudioClip lostSound;
    public AudioClip gotHit;
   

    [Header("Hit")]
    public float hitForce = 10f;
    public float despawnDelay = 5f;

    [HideInInspector]
    public static float deadAI = 0;
    public static float totalAI = 0;
    public static bool playerIsHitByAI = false;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        

        // dostane pocet AI ve hre
        // na kazdy AI ve vazan tento script
        totalAI++;
    }

    void Update()
    {
        
        if (isHit)
            return;

        if (isPlayerDetected)
        {
            FollowPlayer();
        }
        else
        {
            PatrolWaypoints();
            DetectPlayer();
        }
    }

    void PatrolWaypoints()
    {
        if (waypoints.Length == 0 || isHit)
            return;

        if (agent.remainingDistance < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void DetectPlayer()
    {
        if (isHit)
            return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (Vector3.Distance(transform.position, player.position) <= detectionRange && angleToPlayer <= fieldOfView)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
            {
                if (hit.transform == player)
                {
                    if (!isPlayerDetected)
                    {
                        isPlayerDetected = true;
                        OnPlayerDetected();
                    }
                }
            }
        }
    }

    void FollowPlayer()
    {
        if (isHit)
            return;

        agent.SetDestination(player.position);
        agent.acceleration = 5f;

        if (Vector3.Distance(transform.position, player.position) > detectionRange)
        {
            isPlayerDetected = false;
            OnPlayerLost();
        }
    }

    void OnPlayerDetected()
    {
        if (audioSource != null && detectionSound != null)
        {
            audioSource.clip = detectionSound;
            audioSource.Play();
        }
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
    }

    void gotHitByPlayer()
    {
        if (audioSource != null && gotHit != null)
        {
            audioSource.clip = gotHit;
            audioSource.Play();
        }
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
    }

    void OnPlayerLost()
    {
        if (audioSource != null && lostSound != null)
        {
            audioSource.clip = lostSound;
            audioSource.Play();
        }
        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
    }

    // hit AI
    public void OnHit()
    {
        isHit = true;
        gotHitByPlayer();

        agent.enabled = false; 
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.AddForce(-transform.forward * hitForce, ForceMode.Impulse);
        }
        StartCoroutine(DestroyAfterDelay(despawnDelay));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        deadAI++;
        Destroy(gameObject);
    }

    // hit playera
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsHitByAI = true;
            Debug.Log("Zasazen Hrac: " + other.name);
        }
    }
}
