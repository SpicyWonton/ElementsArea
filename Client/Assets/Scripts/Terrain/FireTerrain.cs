using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTerrain : BaseTerrain
{
    public float expositionDamage = 20f;        //爆炸伤害
    public Collider parentCollider;
    public GameObject fireObj;                  //与火结合生成的物体 
    public GameObject waterObj;                 //与水结合生成的物体
    public bool firstGround = true;             //是否是第一次接触地面  生成时降落到地面算第一次
    public bool openMul = false;                //是否开启伤害加倍
    public bool onlyOne = false;                //只执行一次 true的时候可以执行
    public PlayerManager playerPM = null;              //持有此块的玩家
    

    // Start is called before the first frame update
    void Start()
    {
        parentCollider = transform.parent.GetComponent<Collider>();
    }
    private void Update()
    {
        currTime += Time.deltaTime;
        //射线检测 当此地板距离地面小于0.2 且 是第一次时触发 将触发器设置为碰撞体（初始是触发器是为了让地板能正确穿透玩家到达地面
        Ray ray = new Ray(transform.parent.transform.position, Vector3.down);
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, 0.4f) && firstGround)
        {
            if(raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                parentCollider.isTrigger = false;
                firstGround = false;
                onlyOne = true;
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        //与火结合 产生剧烈火焰地表
        if (other.tag =="FieryPit" && onlyOne)
        {
            onlyOne = false;
            Debug.Log(other.GetComponentInChildren<FireTerrain>().playerPM);
            GameObject tempObj = Instantiate(fireObj);
            //生成的火焰地表归属于对方（后扔的
            FireTerrain2 gameObject = tempObj.GetComponentInChildren<FireTerrain2>();
            gameObject.playerPM = other.GetComponentInChildren<FireTerrain>().playerPM;


            tempObj.transform.position = transform.position;
            Destroy(other.gameObject);
            Destroy(transform.parent.gameObject);

            //销毁自己
            Destroy(transform.parent.gameObject);
        }
        //与雷结合 产生爆炸
        else if (other.tag == "ThunderPit" && onlyOne)
        {
            onlyOne = false;
            //实例化火焰特效
            GameObject particle = ObjectPool.instance.TakeParticle("ExpMedium(Clone)");
            particle.transform.position = transform.parent.transform.position;
            Collider[] colliders =  Physics.OverlapSphere(particle.transform.position,3f);
            foreach (Collider col in colliders)
            {
                //如果是玩家 造成伤害
                if (col.tag == "Player")
                {
                    //如果该玩家血量已经小于0了 下一轮
                    if(col.gameObject.GetComponent<PlayerManager>().currentHp <= 0)
                    {
                        continue;
                    }
                    col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(100f,particle.transform.position,3f,0f,ForceMode.Impulse);
                    col.gameObject.GetComponent<PlayerManager>().AddHp(-expositionDamage);

                    //如果对象血量小于爆炸伤害值 就是打死 加分
                    if (col.gameObject.GetComponent<PlayerManager>().currentHp <= 0)
                    {
                        //击杀播报
                        string uid1 = other.GetComponentInChildren<ThunderTerrain>().playerPM.myUID;//杀人者id 是看后来扔的那个人算伤害
                        string uid2 = col.gameObject.GetComponent<PlayerManager>().myUID;//被杀者id
                        GameManager.UpdateKillRadio(uid1, uid2, GameManager.KillType.Burn);
                        if (uid1 != uid2)
                        {
                            //加分UI
                            UIManager.UpdateAddPoint(other.GetComponentInChildren<ThunderTerrain>().playerPM, 2);
                            //计分
                            other.GetComponentInChildren<ThunderTerrain>().playerPM.GetSocre(2);
                        }
                    }
                }
            }
            Destroy(other.gameObject);
            //销毁自己
            Destroy(transform.parent.gameObject);
        }
        //与水结合 生成烟雾
        else if(other.tag == "WaterPit" && onlyOne)
        {
            onlyOne = false;
            GameObject tempObj = Instantiate(waterObj);
            tempObj.transform.position = transform.position;
            Destroy(other.gameObject);
            Destroy(transform.parent.gameObject);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        //是否是玩家
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //玩家是否处于着火状态
            if (!other.gameObject.GetComponent<PlayerManager>().GetPlayerFire())
            {
                // //如果开启了伤害加倍就伤害加倍
                // if(openMul)
                // {
                //     other.gameObject.GetComponent<PlayerManager>().SetPlayerFireDamageMul(grassDamageMul);
                // }
                //不处于把着火状态设置为true
                other.gameObject.GetComponent<PlayerManager>().SetPlayerFireDamageMul(1);
                other.gameObject.GetComponent<PlayerManager>().SetPlayerFire(true, playerPM.gameObject);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.gameObject.GetComponent<PlayerManager>().SetPlayerFire(false, null);
        }
    }
}
