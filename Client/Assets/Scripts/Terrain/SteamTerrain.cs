using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SteamTerrain : MonoBehaviour
{
    public GameObject blockPanel;//烟雾UI
    public float time = 3f;//持续时间

    void Start()
    {
        blockPanel = GameObject.Find("Canvas/BlockPanel");
    }
    private void OnTriggerEnter(Collider other)
    {
        //如果是玩家 并且玩家是自己 触发烟雾效果
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //判断玩家是否处于烟雾状态
            if(!other.gameObject.GetComponent<PlayerManager>().GetPlayerSmoke())
            {
                other.gameObject.GetComponent<PlayerManager>().SetPlayerSmoke(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //如果是玩家 结束烟雾效果
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.gameObject.GetComponent<PlayerManager>().SetPlayerSmoke(false);
        }
    }
}
