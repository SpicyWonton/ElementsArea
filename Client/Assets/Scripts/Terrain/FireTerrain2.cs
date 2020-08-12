using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTerrain2 : BaseTerrain
{
    public Collider parentCollider;
    
    [Tooltip("火焰伤害倍率 比如说2倍mul就是2")]
    public float fireDamageMul;//与火结合生成燃烧的伤害倍率
    public bool firstGround = true;//是否是第一次接触地面  生成时降落到地面算第一次
    public bool openMul = false;//是否开启伤害加倍

    public bool onlyOne = true;//只执行一次 true的时候可以执行

    public PlayerManager playerPM = null;              //持有此块的玩家

    // Start is called before the first frame update
    void Start()
    {
        parentCollider = transform.parent.GetComponent<Collider>();
    }
    private void Update()
    {
        currTime += Time.deltaTime;
        //射线检测 当此地板距离地面小于0.5 且 是第一次时触发 将触发器设置为碰撞体（初始是触发器是为了让地板能正确穿透玩家到达地面
        Ray ray = new Ray(transform.parent.transform.position, Vector3.down);
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, 0.5f) && firstGround)
        {
            if(raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                parentCollider.isTrigger = false;
                firstGround = false;
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {

    }


    private void OnTriggerStay(Collider other)
    {
        //是否是玩家
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //玩家是否处于着火状态
            if (!other.gameObject.GetComponent<PlayerManager>().GetPlayerFire())
            {
                //伤害加倍
                other.gameObject.GetComponent<PlayerManager>().SetPlayerFireDamageMul(fireDamageMul);
                //不处于把着火状态设置为true
                other.gameObject.GetComponent<PlayerManager>().SetPlayerFire(true,playerPM.gameObject);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.gameObject.GetComponent<PlayerManager>().SetPlayerFire(false,null);
        }
    }
}
