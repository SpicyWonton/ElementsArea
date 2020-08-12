using GrpcLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllCube : Cube
{
    public int cid;                                 //每个方块的id，同步需要用
    public AudioClip[] hitClips;                    //砸中播放的音效
    public GameObject[] prefabs;                    //需要实例化的地形
    public Dictionary<cubeType, int> weaponType;    //根据方块类型指定地形类型
    public ParticleSystem weaponParticle;           //粒子特效
    public bool onlyOne = true;                     //只运行一次
    public PlayerManager playerPM;                  //持有此块的玩家

    private void Awake()
    {
        weaponType = new Dictionary<cubeType, int>();
    }

    private void Start()
    {
        //测试数据 要删除
        //openDamage = true;
        //添加脚本的武器类型
        weaponType.Add(cubeType.Terra, 0);
        weaponType.Add(cubeType.Wood, 1);
        weaponType.Add(cubeType.Water, 2);
        weaponType.Add(cubeType.Ice, 3);
        weaponType.Add(cubeType.Fire, 4);
        weaponType.Add(cubeType.Thunder, 5);

        cid = CubeManager.GetCID();             //分配cid
        CubeManager.AddCube(cid, gameObject);   //纳入管理
    }

    //玩家调用
    public void SetplayerPM(PlayerManager pm)
    {
        playerPM = pm;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        //是否碰撞全交给房主计算，保持一致性
        if (User.isHost)
        {
            Vector3 tempVec = gameObject.transform.position;
            //开启伤害之后才执行
            if (openDamage && onlyOne)
            {
                //向服务器发送消息
                Vector cubePos = new Vector
                {
                    X = tempVec.x,
                    Y = tempVec.y,
                    Z = tempVec.z
                };
                //打到人了 并且不是我自己
                if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && collision.gameObject.GetComponent<PlayerManager>().myPID != playerPM.myPID ) 
                {
                    //向服务器发送消息
                    CubeColl msg = new CubeColl
                    {
                        CID = cid,
                        CubePos = cubePos,
                        PID = collision.gameObject.GetComponent<PlayerManager>().myPID,
                        Flag = true
                    };
                    Client.SendMsg((uint)MSGID.Collision, msg);

                    onlyOne = false;

                    //生成特效
                    ParticleSystem temp = Instantiate(weaponParticle);
                    temp.transform.position = collision.transform.position;
                    //生成音效
                    collision.gameObject.GetComponent<PlayerManager>().SetAudioClip(audioClip);

                    //如果对象血量小于伤害值 并且不是AI 就是打死 加分
                    if (collision.gameObject.GetComponent<PlayerManager>().currentHp <= damage && !collision.gameObject.GetComponent<PlayerManager>().isAI)
                    {
                        //击杀播报
                        string uid1 = playerPM.myUID;
                        string uid2 = collision.gameObject.GetComponent<PlayerManager>().myUID;
                        GameManager.UpdateKillRadio(uid1, uid2, GameManager.KillType.Hit);
                        if (uid1 != uid2)
                        {
                            //加分UI
                            UIManager.UpdateAddPoint(playerPM, 2);
                            //不是自杀，计分
                            playerPM.GetSocre(2);
                        }
                    }

                    //如果是火焰 进入燃烧状态
                    if (cubeWeapType == cubeType.Fire)
                    {
                        collision.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
                        collision.gameObject.GetComponent<PlayerManager>().SetPlayerFire(true, playerPM.gameObject);
                    }
                    //如果是水 进入潮湿状态
                    else if (cubeWeapType == cubeType.Water)
                    {
                        collision.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
                        collision.gameObject.GetComponent<PlayerManager>().SetPlayerWet(true);
                    }
                    else if (cubeWeapType == cubeType.Terra)
                    {
                        collision.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
                    }
                    //如果是雷属性 掉血 进入麻痹状态
                    else if (cubeWeapType == cubeType.Thunder)
                    {
                        collision.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
                        collision.gameObject.GetComponent<PlayerManager>().SetPlayerParalysis(true);
                    }

                    Destroy(gameObject);
                }
                //打到地板 且不是土块 生成新地形
                else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && onlyOne)
                {
                    //向服务器发送消息
                    CubeColl msg = new CubeColl
                    {
                        CID = cid,
                        CubePos = cubePos,
                        PID = -1,
                        Flag = false
                    };
                    Client.SendMsg((uint)MSGID.Collision, msg);

                    if (cubeWeapType != cubeType.Terra)
                    {
                        onlyOne = false;
                        var temp = Instantiate(prefabs[weaponType[cubeWeapType]], tempVec + Vector3.up, new Quaternion());
                        if(temp.GetComponentInChildren<FireTerrain>())
                        {
                            //如果是火给火赋pm
                            FireTerrain[] fireObjects = temp.GetComponentsInChildren<FireTerrain>();
                            foreach(FireTerrain gameObj in fireObjects)
                            {
                                gameObj.playerPM = this.playerPM;
                            } 
                        }
                        if(temp.GetComponentInChildren<ThunderTerrain>())
                        {
                            //如果是雷给雷赋pm
                            ThunderTerrain[] thunderObjects = temp.GetComponentsInChildren<ThunderTerrain>();
                            foreach(ThunderTerrain gameObj in thunderObjects)
                            {
                                gameObj.playerPM = this.playerPM;
                            } 
                        }
                        temp.transform.SetParent(null);
                        Destroy(gameObject);
                    }
                    else
                    {
                        onlyOne = false;
                        //生成特效
                        ParticleSystem temp = Instantiate(weaponParticle);
                        temp.transform.position = gameObject.transform.position;
                        //生成音效
                        playerPM.SetAudioClip(hitClips[0]);
                        Destroy(gameObject);
                    }
                }
            }
        }
        else if (User.isSingle)
        {
            Vector3 tempVec = gameObject.transform.position;
            //开启伤害之后才执行
            if (openDamage && onlyOne)
            {
                //打到人了 并且不是我自己
                if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && collision.gameObject.GetComponent<PlayerManager>().myPID != playerPM.myPID)
                {
                    onlyOne = false;

                    //生成特效
                    ParticleSystem temp = Instantiate(weaponParticle);
                    temp.transform.position = collision.transform.position;
                    //生成音效
                    collision.gameObject.GetComponent<PlayerManager>().SetAudioClip(audioClip);

                    //如果对象血量小于伤害值 并且不是AI 就是打死 加分
                    if (collision.gameObject.GetComponent<PlayerManager>().currentHp <= damage && collision.gameObject.GetComponent<PlayerManager>().isAI)
                    {
                        Debug.Log("击杀电脑");
                    }

                    //如果是火焰 进入燃烧状态
                    if (cubeWeapType == cubeType.Fire)
                    {
                        collision.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
                        collision.gameObject.GetComponent<PlayerManager>().SetPlayerFire(true, playerPM.gameObject);
                    }
                    //如果是水 进入潮湿状态
                    else if (cubeWeapType == cubeType.Water)
                    {
                        collision.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
                        collision.gameObject.GetComponent<PlayerManager>().SetPlayerWet(true);
                    }
                    else if (cubeWeapType == cubeType.Terra)
                    {
                        collision.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
                    }
                    //如果是雷属性 掉血 进入麻痹状态
                    else if (cubeWeapType == cubeType.Thunder)
                    {
                        collision.gameObject.GetComponent<PlayerManager>().AddHp(-damage);
                        collision.gameObject.GetComponent<PlayerManager>().SetPlayerParalysis(true);
                    }

                    Destroy(gameObject);
                }
                //打到地板 且不是土块 生成新地形
                else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && onlyOne)
                {
                    if (cubeWeapType != cubeType.Terra)
                    {
                        onlyOne = false;
                        var temp = Instantiate(prefabs[weaponType[cubeWeapType]], tempVec + Vector3.up, new Quaternion());
                        if (temp.GetComponentInChildren<FireTerrain>())
                        {
                            //如果是火给火赋pm
                            FireTerrain[] fireObjects = temp.GetComponentsInChildren<FireTerrain>();
                            foreach (FireTerrain gameObj in fireObjects)
                            {
                                gameObj.playerPM = this.playerPM;
                            }
                        }
                        if (temp.GetComponentInChildren<ThunderTerrain>())
                        {
                            //如果是雷给雷赋pm
                            ThunderTerrain[] thunderObjects = temp.GetComponentsInChildren<ThunderTerrain>();
                            foreach (ThunderTerrain gameObj in thunderObjects)
                            {
                                gameObj.playerPM = this.playerPM;
                            }
                        }
                        temp.transform.SetParent(null);
                        Destroy(gameObject);
                    }
                    else
                    {
                        onlyOne = false;
                        //生成特效
                        ParticleSystem temp = Instantiate(weaponParticle);
                        temp.transform.position = gameObject.transform.position;
                        //生成音效
                        playerPM.SetAudioClip(hitClips[0]);
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
