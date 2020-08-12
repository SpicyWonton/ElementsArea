using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderTerrain : BaseTerrain
{
    public float expositionDamage = 20f;//爆炸伤害
    public Collider parentCollider;
    public GameObject thunderObj;//与雷结合生成的物体
    public bool firstGround = true;//是否是第一次接触地面  生成时降落到地面算第一次
    public bool openMul = false;//是否开启伤害加倍
    public bool onlyOne = true;//只执行一次 true的时候可以执行
    public PlayerManager playerPM = null;//归属人的pm
    

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
        if (Physics.Raycast(ray, out raycastHit, 0.4f) && firstGround)
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
        //与火结合 产生爆炸
        if(other.tag =="FieryPit" && onlyOne)
        {
            onlyOne = false;
            //实例化火焰特效
            GameObject particle = ObjectPool.instance.TakeParticle("ExpMedium(Clone)");
            particle.transform.position = transform.parent.transform.position;
            Collider[] colliders =  Physics.OverlapSphere(particle.transform.position,3f);
            foreach(Collider col in colliders)
            {
                //如果是玩家 造成伤害
                if(col.tag == "Player")
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
        //与雷结合 加强
        else if(other.tag == "ThunderPit" && onlyOne)
        {
            onlyOne = false; 
            GameObject tempObj = Instantiate(thunderObj);
            Destroy(other.gameObject);
            tempObj.transform.position = transform.position;
            Destroy(transform.parent.gameObject);
            //销毁自己
            Destroy(transform.parent.gameObject);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        //是否是玩家
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //玩家是否处于麻痹状态
            if (!other.gameObject.GetComponent<PlayerManager>().GetPlayerParalysis())
            {
                //不处于麻痹状态设置为true
                other.gameObject.GetComponent<PlayerManager>().SetPlayerParalysis(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.gameObject.GetComponent<PlayerManager>().SetPlayerParalysis(true);
        }
    }
}
