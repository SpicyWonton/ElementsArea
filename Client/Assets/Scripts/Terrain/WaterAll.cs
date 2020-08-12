using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAll : MonoBehaviour
{
    public bool onlyOne = true;//只执行一次
    public GameObject thunderObj;//雷水地表
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        //如果是雷 产生雷水地表
        if(other.gameObject.tag == "ThunderPit" && onlyOne)
        {
            onlyOne = false;
            //Debug.Log(other.GetComponentInParent<Transform>().parent.gameObject.name);
            GameObject otherParent = other.GetComponentInParent<Transform>().parent.gameObject;
            GameObject tempObj1 = Instantiate(thunderObj);
            tempObj1.transform.position = otherParent.transform.position;
            Destroy(otherParent);
            GameObject tempObj2 = Instantiate(thunderObj);
            tempObj2.transform.position = transform.position;
            //销毁自己
            Destroy(gameObject);
        }     
    }    
}
