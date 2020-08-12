using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDead : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        //如果是玩家
        if(other.gameObject.GetComponent<PlayerManager>())
        {
            other.gameObject.GetComponent<PlayerManager>().AddHp(-1000);
        }
    }

}
