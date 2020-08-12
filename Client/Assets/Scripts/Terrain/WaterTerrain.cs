using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTerrain : BaseTerrain
{
    public Collider parentCollider;
    public GameObject water2;//水块2
    public GameObject fireObj;//与火结合
    public GameObject thunderObj;//与雷结合 雷水地表
    public bool first = true;
    public bool onlyOne = true;//碰撞合成只产生一次
    // Start is called before the first frame update
    void Start()
    {
        parentCollider = transform.parent.GetComponent<Collider>();
    }
    private void Update()
    {
        currTime += Time.deltaTime;

        //射线检测 当此地板距离地面小于0.1 且 是第一次时触发 将触发器设置为碰撞体（初始是触发器是为了让地板能正确穿透玩家到达地面
        Ray ray = new Ray(transform.parent.transform.position, Vector3.down);
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, 0.4f) && first)
        {
            if (raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                parentCollider.isTrigger = false;
                first = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //如果是水块  生成水2
        if (other.gameObject.tag == "WaterPit" && onlyOne )
        {
            onlyOne = false;
            GameObject tempObj = Instantiate(water2);
            Destroy(other.gameObject);
            tempObj.transform.position = transform.position;
            Destroy(transform.parent.gameObject);
            //销毁自己
            Destroy(transform.parent.gameObject);
        }
        //如果是火 产生蒸汽
        else if(other.gameObject.tag == "FieryPit" && onlyOne)
        {
            onlyOne = false;
            GameObject tempObj = Instantiate(fireObj);
            Destroy(other.gameObject);
            tempObj.transform.position = transform.position;
            Destroy(transform.parent.gameObject);
            //销毁自己
            Destroy(transform.parent.gameObject);
        }
        // //如果是雷 产生雷水地表
        // else if(other.gameObject.tag == "ThunderPit" && onlyOne)
        // {
        //     onlyOne = false;
        //     //Debug.Log(other.GetComponentInParent<Transform>().parent.gameObject.name);
        //     GameObject otherParent = other.GetComponentInParent<Transform>().parent.gameObject;
        //     GameObject tempObj1 = Instantiate(thunderObj);
        //     tempObj1.transform.position = otherParent.transform.position;
        //     Destroy(otherParent);
        //     GameObject tempObj2 = Instantiate(thunderObj);
        //     tempObj2.transform.position = GetComponentInParent<Transform>().parent.transform.position;
        //     //销毁自己
        //     Destroy(transform.parent.parent.gameObject);
        // }     
    }    

    private void OnTriggerStay(Collider other)
    {
        //是否是玩家
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //玩家是否处于潮湿状态
            if (!other.gameObject.GetComponent<PlayerManager>().GetPlayerWet())
            {
                //不处于把潮湿状态设置为true
                other.gameObject.GetComponent<PlayerManager>().SetPlayerWet(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.gameObject.GetComponent<PlayerManager>().SetPlayerWet(false);
        }
    }


    // //保持里面的时候 禁止攻击
    // private void OnTriggerStay(Collider other)
    // {
    //     //如果是玩家
    //     if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
    //     {
    //         other.gameObject.GetComponent<PlayerManager>().SetPlayerBanAttack(true);
    //     }        
    // }
    // //退出的时候 允许攻击
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
    //     {
    //         other.gameObject.GetComponent<PlayerManager>().SetPlayerBanAttack(false);
    //     }
    // }
}
