using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAll : MonoBehaviour
{

    public GameObject fireObj;//与火结合生成的物体 
    public bool onlyOne = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // private void OnTriggerEnter(Collider other)
    // {
    //     Debug.Log(other.name);
    //     if(other.gameObject.tag == "FieryPit" && onlyOne)
    //     {
    //         onlyOne = false;
    //         Destroy(other.gameObject);
    //         GameObject obj = Instantiate(fireObj);
    //         obj.transform.position = this.transform.position;
    //         Destroy(this.gameObject);
    //     }
    // }
}
