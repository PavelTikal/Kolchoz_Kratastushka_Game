using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{

    private Rigidbody rb;
    private Collider collider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }

        rb.isKinematic = true; 
    }

    public void OnHitTarget()
    {
        Debug.Log("Target hit!");
        rb.isKinematic = false; 
    }
}
