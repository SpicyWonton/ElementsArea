using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassTerrain : BaseTerrain
{

    public Collider parentCollider;
    public GameObject grass2;//草+草合成的
    public bool first = true;
    public bool isGrass2;//是不是草2
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
        if (Physics.Raycast(ray, out raycastHit, 0.5f) && first)
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
        if(!isGrass2)
        {
            //如果是草  生成草2
            if (other.gameObject.tag == "GrassPit" )
            {
                
                //Debug.Log("cao2!");
                GameObject tempObj = Instantiate(grass2);
                Destroy(other.gameObject);
                tempObj.transform.position = (transform.position + other.gameObject.transform.position)/2;
                Destroy(transform.parent.gameObject);
            }     
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(isGrass2)
        {
            //是否是玩家
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                //玩家是否处于复苏
                if (!other.gameObject.GetComponent<PlayerManager>().GetPlayerRes())
                {
                    //不处于把复苏状态设置为true
                    other.gameObject.GetComponent<PlayerManager>().SetPlayerRes(true);
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(isGrass2)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                other.gameObject.GetComponent<PlayerManager>().SetPlayerRes(true);
            }
        }
    }
}
