using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsBase : MonoBehaviour
{    
    public int Value;
    public float durationTime;  //销毁时间
    
    private void Start()
    {
        //一段时间后自动销毁
        Destroy(gameObject, durationTime);
    }
}
