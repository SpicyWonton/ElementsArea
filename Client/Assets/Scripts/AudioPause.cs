using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPause : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(GameObject.Find("BackGroundAudio"))
        {
            GameObject.Find("BackGroundAudio").SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
