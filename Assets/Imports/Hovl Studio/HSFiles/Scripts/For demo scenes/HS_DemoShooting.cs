﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class HS_DemoShooting : MonoBehaviour
{
    [Header("Object pool parameters")]
    public int poolSize = 20;
    public Dictionary<int, Queue<GameObject>> poolDictionary;
    public Dictionary<int, Queue<GameObject>> flashPoolDictionary;
    public float poolReturnTimer = 3f;

    public GameObject FirePoint;
    public Camera Cam;
    public float MaxLength;
    public GameObject[] Prefabs;
    public GameObject[] Flashes;

    private Ray RayMouse;
    private Vector3 direction;
    private Quaternion rotation;

    [Header("GUI")]
    private float windowDpi;
    private int Prefab;
    private GameObject Instance;
    private float hSliderValue = 0.1f;
    private float fireCountdown = 0f;

    //Double-click protection
    private float buttonSaver = 0f;

    //For Camera shake 
    public Animation camAnim;

    void Awake()
    {
        int t = 0;
        int ft = 0;
        poolDictionary = new Dictionary<int, Queue<GameObject>>();
        foreach (GameObject pool in Prefabs)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(pool, transform, true);
                //obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(t, objectPool);
            t++;
        }

        flashPoolDictionary = new Dictionary<int, Queue<GameObject>>();
        foreach (GameObject pool in Flashes)
        {
            if (pool != null)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                for (int i = 0; i < poolSize; i++)
                {
                    GameObject obj = Instantiate(pool, transform, true);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                flashPoolDictionary.Add(ft, objectPool);
            }
            ft++;
        }

        if (Screen.dpi < 1) windowDpi = 1;
        if (Screen.dpi < 200) windowDpi = 1;
        else windowDpi = Screen.dpi / 200f;
        Counter(0);
    }

    public void GetPooledObjects(int count, Vector3 position, Quaternion rotation)
    {
        GameObject objectToSpawn = poolDictionary[count].Dequeue();
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        //objectToSpawn.SetActive(true);
        objectToSpawn.transform.parent = null;
        poolDictionary[count].Enqueue(objectToSpawn);
    }

    public void GetPooledFlashes(int count, Vector3 position, Quaternion rotation)
    {
        if (flashPoolDictionary[count] != null)
        {
            GameObject flashToSpawn = flashPoolDictionary[count].Dequeue();
            flashToSpawn.transform.position = position;
            flashToSpawn.transform.rotation = rotation;
            flashToSpawn.SetActive(true);
            flashPoolDictionary[count].Enqueue(flashToSpawn);
            StartCoroutine(FlashCall(flashToSpawn));
        }
    }

    private IEnumerator FlashCall(GameObject flash)
    {
        yield return new WaitForSeconds(1);
        flash.SetActive(false);
        yield break;
    }

    void Update()
    {
        //Single shoot
        if (Input.GetButtonDown("Fire1"))
        {
            camAnim.Play(camAnim.clip.name);
            GetPooledObjects(Prefab, FirePoint.transform.position, FirePoint.transform.rotation);
            if(flashPoolDictionary.ContainsKey(Prefab))
                GetPooledFlashes(Prefab, FirePoint.transform.position, FirePoint.transform.rotation);
        }

        //Fast shooting
        if (Input.GetMouseButton(1) && fireCountdown <= 0f)
        {
            GetPooledObjects(Prefab, FirePoint.transform.position, FirePoint.transform.rotation);
            if (flashPoolDictionary.ContainsKey(Prefab))
                GetPooledFlashes(Prefab, FirePoint.transform.position, FirePoint.transform.rotation);
            fireCountdown = 0;
            fireCountdown += hSliderValue;
        }
        fireCountdown -= Time.deltaTime;

        //To change projectiles
        if ((Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < 0) && buttonSaver >= 0.4f)// left button
        {
            buttonSaver = 0f;
            Counter(-1);
        }
        if ((Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0) && buttonSaver >= 0.4f)// right button
        {
            buttonSaver = 0f;
            Counter(+1);
        }
        buttonSaver += Time.deltaTime;

        //To rotate fire point
        if (Cam != null)
        {
            RaycastHit hit;
            var mousePos = Input.mousePosition;
            RayMouse = Cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(RayMouse.origin, RayMouse.direction, out hit, MaxLength))
            {
                RotateToMouseDirection(gameObject, hit.point);
            }
        }
        else
        {
            Debug.Log("No camera");
        }
    }

    //GUI Text
    void OnGUI()
    {
        GUI.Label(new Rect(10 * windowDpi, 5 * windowDpi, 400 * windowDpi, 20 * windowDpi), "Use left mouse button to single shoot!");
        GUI.Label(new Rect(10 * windowDpi, 25 * windowDpi, 400 * windowDpi, 20 * windowDpi), "Use and hold the right mouse button for quick shooting!");
        GUI.Label(new Rect(10 * windowDpi, 45 * windowDpi, 400 * windowDpi, 20 * windowDpi), "Fire rate:");
        hSliderValue = GUI.HorizontalSlider(new Rect(70 * windowDpi, 50 * windowDpi, 100 * windowDpi, 20 * windowDpi), hSliderValue, 0.0f, 1.0f);
        GUI.Label(new Rect(10 * windowDpi, 65 * windowDpi, 400 * windowDpi, 20 * windowDpi), "Use the keyboard buttons A/<- and D/-> to change projectiles!");
    }

    // To change prefabs (count - prefab number)
    void Counter(int count)
    {
        Prefab += count;
        if (Prefab > Prefabs.Length - 1)
        {
            Prefab = 0;
        }
        else if (Prefab < 0)
        {
            Prefab = Prefabs.Length - 1;
        }
    }

    //To rotate fire point
    void RotateToMouseDirection(GameObject obj, Vector3 destination)
    {
        direction = destination - obj.transform.position;
        rotation = Quaternion.LookRotation(direction);
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, rotation, 1);
    }
}
