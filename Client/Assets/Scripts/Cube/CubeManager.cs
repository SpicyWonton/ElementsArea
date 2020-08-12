using GrpcLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : BaseManager<CubeManager>
{
    public Dictionary<int, GameObject> cubes = new Dictionary<int, GameObject>();

    private int currCID = 0;

    protected override void Init()
    {
        
    }

    //获取方块id，方块Start时调用
    public static int GetCID()
    {
        int cid = instance.currCID++;
        return cid;
    }

    //将方块纳入管理中，方块Start时调用
    public static void AddCube(int cid, GameObject cube)
    {
        instance.cubes.Add(cid, cube);
    }

    //收到消息后调用
    public static void UpdateCollision(CubeColl cubeColl)
    {
        GameObject collCube = instance.cubes[cubeColl.CID];
        AllCube allCube = collCube.GetComponent<AllCube>();

        if (allCube.openDamage && allCube.onlyOne)
        {
            if (cubeColl.Flag)  //击中玩家
            {
                allCube.onlyOne = false;

                GameObject collPlayer = GameManager.instance.players[cubeColl.PID];
                //生成特效
                Instantiate(allCube.weaponParticle, collPlayer.transform.position, Quaternion.identity);
                //生成音效
                collPlayer.GetComponent<PlayerManager>().SetAudioClip(allCube.audioClip);

                //如果对象血量小于伤害值 就是打死 加分
                if (collPlayer.GetComponent<PlayerManager>().currentHp <= allCube.damage)
                {
                    //击杀播报
                    string uid1 = allCube.playerPM.myUID;
                    string uid2 = collPlayer.GetComponent<PlayerManager>().myUID;
                    GameManager.UpdateKillRadio(uid1, uid2, GameManager.KillType.Hit);
                    if (uid1 != uid2)
                    {
                        //加分UI
                        UIManager.UpdateAddPoint(allCube.playerPM, 2);
                        //不是自杀，计分  
                        allCube.playerPM.GetSocre(2);
                    }  
                }

                //如果是火焰 进入燃烧状态
                if (allCube.cubeWeapType == Cube.cubeType.Fire)
                {
                    collPlayer.GetComponent<PlayerManager>().AddHp(-allCube.damage);
                    collPlayer.GetComponent<PlayerManager>().SetPlayerFire(true, allCube.playerPM.gameObject);
                }
                //如果是水 进入潮湿状态
                else if (allCube.cubeWeapType == Cube.cubeType.Water)
                {
                    collPlayer.GetComponent<PlayerManager>().AddHp(-allCube.damage);
                    collPlayer.GetComponent<PlayerManager>().SetPlayerWet(true);
                }
                else if (allCube.cubeWeapType == Cube.cubeType.Terra)
                {
                    collPlayer.GetComponent<PlayerManager>().AddHp(-allCube.damage);
                }
                //如果是雷属性 掉血 进入麻痹状态
                else if (allCube.cubeWeapType == Cube.cubeType.Thunder)
                {
                    collPlayer.GetComponent<PlayerManager>().AddHp(-allCube.damage);
                    collPlayer.GetComponent<PlayerManager>().SetPlayerParalysis(true);
                }

                Destroy(collCube);
            }
            else                //击中地面
            {
                if (allCube.cubeWeapType != Cube.cubeType.Terra)
                {
                    allCube.onlyOne = false;
                    Vector3 cubePos = new Vector3(cubeColl.CubePos.X, cubeColl.CubePos.Y, cubeColl.CubePos.Z);
                    var temp = Instantiate(allCube.prefabs[allCube.weaponType[allCube.cubeWeapType]], cubePos + Vector3.up, new Quaternion());
                    
                    //如果是火给火赋pm
                    if(temp.GetComponentInChildren<FireTerrain>())
                    {
                        FireTerrain[] fireObjects = temp.GetComponentsInChildren<FireTerrain>();
                        foreach(FireTerrain gameObj in fireObjects)
                        {
                            gameObj.playerPM = allCube.playerPM;
                        } 
                    }
                    //如果是雷给雷赋pm
                    if(temp.GetComponentInChildren<ThunderTerrain>())
                    {
                        ThunderTerrain[] thunderObjects = temp.GetComponentsInChildren<ThunderTerrain>();
                        foreach(ThunderTerrain gameObj in thunderObjects)
                        {
                            gameObj.playerPM = allCube.playerPM;
                        } 
                    }
                    temp.transform.SetParent(null);
                    Destroy(collCube);
                }
                else
                {
                    allCube.onlyOne = false;
                    //生成特效
                    ParticleSystem temp = Instantiate(allCube.weaponParticle);
                    temp.transform.position = collCube.transform.position;
                    //生成音效
                    allCube.playerPM.SetAudioClip(allCube.hitClips[0]);
                    Destroy(collCube);
                }
            }
        } 
    }
}
