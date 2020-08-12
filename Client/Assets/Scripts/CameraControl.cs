using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Vector3 offset;
    public Transform hero;

    private void Awake()
    {
        // hero = GameObject.Find("Hero").GetComponent<Transform>();
    }

    private void LateUpdate()
    {
        
        transform.position = hero.position + offset;
    }
}
