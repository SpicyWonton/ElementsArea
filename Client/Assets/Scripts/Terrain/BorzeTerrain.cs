using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorzeTerrain : MonoBehaviour
{
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
        //如果是玩家 触发冻结状态
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //玩家是否处于冻结状态
            if (!other.gameObject.GetComponent<PlayerManager>().GetPlayerBroze())
            {
                //不处于冻结状态设置为true
                other.gameObject.GetComponent<PlayerManager>().SetPlayerBorze(true);
            }
        }
    }
}
