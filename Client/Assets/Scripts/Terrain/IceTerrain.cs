using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceTerrain : BaseTerrain
{
    public Collider parentCollider;
    public bool first = true;
    public GameObject borzeObj;//加水的物体
    public GameObject iceWallObj;//加冰的墙
    public GameObject fireObj;//加火的物体
    public GameObject grassObj;//加草的物体

    public bool isStatic = false;//是不是固定的物体

    // Start is called before the first frame update
    void Start()
    {
        parentCollider = transform.parent.GetComponent<Collider>();
    }
    private void Update()
    {
        currTime += Time.deltaTime;

        if(!isStatic)
        {
            //射线检测 当此地板距离地面小于0.1 且 是第一次时触发 将触发器设置为碰撞体（初始是触发器是为了让地板能正确穿透玩家到达地面
            Ray ray = new Ray(transform.parent.transform.position, Vector3.down);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, 0.5f) && first)
            {
                if (raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    parentCollider.isTrigger = false;
                    first = false;
                }
            }
        }
        else if(isStatic)
        {
            parentCollider.isTrigger = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Transform temp = other.gameObject.GetComponent<PlayerControl>().model.transform;
            other.gameObject.GetComponent<Rigidbody>().AddForce(temp.forward*iceMoveSpeed, ForceMode.Force); 
        }
    }

    // ////进入的时候滑行
    // private void OnTriggerEnter(Collider other)
    // {
    //     //如果是玩家
    //     if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
    //     {
    //         //如果没有滑行 就滑
    //         if(!other.gameObject.GetComponent<PlayerManager>().GetPlayerSlider())
    //         {
    //             other.gameObject.GetComponent<PlayerManager>().SetPlayerSlider(true);
    //             //Transform temp = other.gameObject.GetComponent<PlayerControl>().model.transform;
    //             //other.gameObject.GetComponent<Rigidbody>().AddForce(temp.forward*iceMoveSpeed, ForceMode.Impulse);
    //         }
    //         //Debug.Log("yes");
    //         //var temp = other.gameObject.GetComponent<PlayerControl>().model;
    //         //other.gameObject.GetComponent<Rigidbody>().AddForce(temp.transform.forward * forceMagnitude, ForceMode.Acceleration);
    //         ////禁止移动
    //         //other.gameObject.GetComponent<PlayerManager>().SetPlayerBanMove(true);
    //     }
    //     //如果是水 冰冻
    //     else if(other.tag == "WaterPit")
    //     {   
    //         Debug.Log("borze!");
    //         GameObject tempObj = Instantiate(borzeObj);
    //         Destroy(other.gameObject);
    //         tempObj.transform.position = (transform.position + other.gameObject.transform.position)/2;
    //         Destroy(transform.parent.gameObject);
    //     }
    //     //如果是冰 变成冰墙
    //     else if(other.tag == "IcePit")
    //     {
    //         GameObject tempObj = Instantiate(iceWallObj);
    //         Destroy(other.gameObject);
    //         tempObj.transform.position = (transform.position + other.gameObject.transform.position)/2;
    //         Destroy(transform.parent.gameObject);
    //     }
    //     //如果是火 变成潮湿地形
    //     else if(other.tag == "FieryPit")
    //     {
    //         GameObject tempObj = Instantiate(fireObj);
    //         Destroy(other.gameObject);
    //         tempObj.transform.position = (transform.position + other.gameObject.transform.position)/2;
    //         Destroy(transform.parent.gameObject);           
    //     }
    //     //如果是草 变成冰棘地形
    //     else if(other.tag == "GrassPit")
    //     {
    //         GameObject tempObj = Instantiate(grassObj);
    //         Destroy(other.gameObject);
    //         tempObj.transform.position = (transform.position + other.gameObject.transform.position)/2;
    //         Destroy(transform.parent.gameObject);   
    //     }
    // }
}
