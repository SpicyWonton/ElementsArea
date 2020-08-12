using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutMe : MonoBehaviour
{
    public float time = 2f;
    private float currTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currTime += Time.deltaTime;
        if(currTime >= time)
        {
            currTime = 0f;
            gameObject.SetActive(false);
        }
    }
}
