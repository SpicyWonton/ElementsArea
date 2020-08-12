using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WTTerrain : BaseTerrain
{
    public Collider parentCollider;

    [Tooltip("水效果倍率 如果想让他减速是普通减速的2倍就0.5")]
    public float waterEffMul = 0.5f;//水效果倍率

    [Tooltip("雷效果倍率 如果频率变化是普通的2倍就是0.5")]
    public float thunderMul = 0.5f;//雷效果倍率
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


    private void OnTriggerStay(Collider other)
    {
        //是否是玩家
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //玩家是否处于潮湿状态
            if (!other.gameObject.GetComponent<PlayerManager>().GetPlayerWet())
            {
                //设置倍率
                other.gameObject.GetComponent<PlayerManager>().SetPlayerWetMul(waterEffMul);
                //不处于把潮湿状态设置为true
                other.gameObject.GetComponent<PlayerManager>().SetPlayerWet(true);
            }
            //玩家是否处于麻痹状态
            if(!other.gameObject.GetComponent<PlayerManager>().GetPlayerParalysis())
            {
                other.gameObject.GetComponent<PlayerManager>().SetPlayerParalysisMul(thunderMul);
                //不处于把麻痹状态设置为true
                other.gameObject.GetComponent<PlayerManager>().SetPlayerParalysis(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.gameObject.GetComponent<PlayerManager>().SetPlayerWetMul(1f);
            other.gameObject.GetComponent<PlayerManager>().SetPlayerParalysisMul(1f);
            other.gameObject.GetComponent<PlayerManager>().SetPlayerWet(false);
            other.gameObject.GetComponent<PlayerManager>().SetPlayerParalysis(false);
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
