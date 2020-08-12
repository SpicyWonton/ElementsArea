using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTerrain2 : BaseTerrain
{
    public Collider parentCollider;
    public float effMul = 0.5f;//效果倍率
    public bool first = true;
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

    // private void OnTriggerEnter(Collider other)
    // {
    //     //如果是水块  添加爆炸力
    //     if (other.gameObject.tag == "WaterPit" )
    //     {
    //         //Debug.Log("exposition!");
    //         Vector3 tempPos = transform.parent.transform.position;
    //         Collider[] colliders =  Physics.OverlapSphere(tempPos,2f);
    //         foreach(var collider in colliders)
    //         {
    //             Rigidbody rb = collider.GetComponent<Rigidbody>();
    //             if(rb!=null)
    //                 rb.AddExplosionForce(2500f,tempPos,3.0f);
    //         }
    //         //销毁自己
    //         // Destroy(transform.parent.gameObject);
    //     }
    //     //如果是冰块 冻结
    //     else if(other.gameObject.tag == "IcePit")
    //     {
    //         GameObject tempObj = Instantiate(iceProduceObj);
    //         Destroy(other.gameObject);
    //         tempObj.transform.position = (transform.position + other.gameObject.transform.position)/2;
    //         Destroy(transform.parent.gameObject);
    //         //销毁自己
    //         Destroy(transform.parent.gameObject);
    //     }     
    //     //如果是火 产生蒸汽
    //     else if(other.gameObject.tag == "FieryPit")
    //     {
    //         GameObject tempObj = Instantiate(fireProduceObj);
    //         Destroy(other.gameObject);
    //         tempObj.transform.position = (transform.position + other.gameObject.transform.position)/2;
    //         Destroy(transform.parent.gameObject);
    //         //销毁自己
    //         Destroy(transform.parent.gameObject);
    //     }
    // }    

    private void OnTriggerStay(Collider other)
    {
        //是否是玩家
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //玩家是否处于潮湿状态
            if (!other.gameObject.GetComponent<PlayerManager>().GetPlayerWet())
            {
                other.gameObject.GetComponent<PlayerManager>().SetPlayerWetMul(effMul);
                //不处于把潮湿状态设置为true
                other.gameObject.GetComponent<PlayerManager>().SetPlayerWet(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.gameObject.GetComponent<PlayerManager>().SetPlayerWetMul(1f);
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
