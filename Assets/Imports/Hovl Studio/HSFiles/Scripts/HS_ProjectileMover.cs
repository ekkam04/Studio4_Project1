﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HS_ProjectileMover : MonoBehaviour
{
    public float dissableAfterTime = 5f;
    public float speed = 15f;
    private float oroginalSpeed;
    public float hitOffset = 0f;
    public bool UseFirePointRotation;
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public GameObject hit;
    public GameObject flash;
    public bool useFlash = false;
    public Rigidbody rb;
    public ParticleSystem ps;
    public Collider sc;
    public Light li;
    public GameObject[] Detached;
    private RigidbodyConstraints originalConstraints;
    public Transform parentObject;

    void Awake()
    {
        if (li != null)
            li.enabled = false;
        sc.enabled = false;
        ps.Stop();
        originalConstraints = rb.constraints;
        oroginalSpeed = speed;
        speed = 0;
        parentObject = transform.parent;
    }

    void Start()
    {
    }

    void OnTransformParentChanged()
    {
        if (parentObject != transform.parent)
        {
            if (li != null)
                li.enabled = true;
            sc.enabled = true;
            rb.constraints = originalConstraints;
            speed = oroginalSpeed;
            ps.Play();
            if (flash != null && useFlash)
            {
                //Instantiate flash effect on projectile position
                var flashInstance = Instantiate(flash, transform.position, Quaternion.identity);
                flashInstance.transform.forward = gameObject.transform.forward;

                //Destroy flash effect depending on particle Duration time
                var flashPs = flashInstance.GetComponent<ParticleSystem>();
                if (flashPs != null)
                {
                    Destroy(flashInstance, flashPs.main.duration);
                }
                else
                {
                    var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(flashInstance, flashPsParts.main.duration);
                }
            }
            
            StartCoroutine(nameof(LateCall));
        }
    }

    void FixedUpdate()
    {
        if (speed != 0)
        {
            rb.velocity = transform.forward * speed;
            //transform.position += transform.forward * (speed * Time.deltaTime);         
        }
    }

    //https ://docs.unity3d.com/ScriptReference/Rigidbody.OnCollisionEnter.html
    void OnCollisionEnter(Collision collision)
    {
        //Lock all axes movement and rotation
        rb.constraints = RigidbodyConstraints.FreezeAll;
        transform.parent = parentObject;
        transform.position = parentObject.position;
        speed = 0;
        sc.enabled = false;
        if (li != null)
            li.enabled = false;
        StopCoroutine(nameof(LateCall));
        ps.Stop();
        foreach (var detachedPrefab in Detached)
        {
            if (detachedPrefab != null)
            {
                detachedPrefab.transform.parent = null;
                StartCoroutine(TouchCall(detachedPrefab));
            }
        }
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point + contact.normal * hitOffset;

        //Spawn hit effect on collision
        if (hit != null)
        {
            var hitInstance = Instantiate(hit, pos, rot);
            if (UseFirePointRotation) { hitInstance.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); }
            else if (rotationOffset != Vector3.zero) { hitInstance.transform.rotation = Quaternion.Euler(rotationOffset); }
            else { hitInstance.transform.LookAt(contact.point + contact.normal); }

            //Destroy hit effects depending on particle Duration time
            var hitPs = hitInstance.GetComponent<ParticleSystem>();
            if (hitPs != null)
            {
                Destroy(hitInstance, hitPs.main.duration);
            }
            else
            {
                var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitInstance, hitPsParts.main.duration);
            }
        }
    }
    
    private IEnumerator LateCall()
    {
        yield return new WaitForSeconds(dissableAfterTime);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        sc.enabled = false;
        if (li != null)
            li.enabled = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        transform.parent = parentObject;
        transform.position = parentObject.position;
        speed = 0;
        yield break;
    }

    private IEnumerator TouchCall(GameObject detachedPrefab)
    {
        yield return new WaitForSeconds(1);
        detachedPrefab.transform.SetParent(gameObject.transform);
        detachedPrefab.transform.position = gameObject.transform.position;
        detachedPrefab.transform.rotation = gameObject.transform.rotation;
        yield break;
    }
}