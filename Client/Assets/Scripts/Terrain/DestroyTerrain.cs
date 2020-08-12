using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTerrain : MonoBehaviour
{
    public float deadTime = 15f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, deadTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
