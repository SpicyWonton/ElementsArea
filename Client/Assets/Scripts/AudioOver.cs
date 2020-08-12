using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioOver : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject audioImg;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
    }

}
