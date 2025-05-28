using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float lifeTime = 2;

    public void Awake()
    {

        if(!TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; 
        }

        if (!TryGetComponent<Collider>(out Collider col))
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true; 
        }

        Destroy(gameObject, lifeTime); 

    }
   
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collision detected with: " + other.gameObject.name); // Debug pro kolize

        if (other.CompareTag("Target"))
        {
            VerbiszkyAI verbiszkyAI = other.GetComponent<VerbiszkyAI>();
            if (verbiszkyAI != null)
            {
                verbiszkyAI.OnHit();
                Debug.Log("Hit AI: " + other.gameObject.name);
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("TTarget"))
        {
            TargetScript targetScript = other.GetComponent<TargetScript>();
            if (targetScript != null)
            {
                targetScript.OnHitTarget();
                Debug.Log("Hit Target: " + other.gameObject.name);
                Destroy(gameObject);
            }
        }
    }
}