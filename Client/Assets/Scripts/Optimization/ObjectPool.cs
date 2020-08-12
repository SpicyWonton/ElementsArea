using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    //单例模式指向自己
    public static ObjectPool instance;

    //特效池
    public Dictionary<string,List<GameObject>> particlePool;
    //需要加入特效池的特效
    public GameObject[] particlesPres;
    //每个特效需要实例化的数量
    public int num = 5;


    //构造函数设置为私有 不允许类的实例化
    private ObjectPool(){}


    //将instance 指向自己
    void Awake(){
        instance = this;
        particlePool = new Dictionary<string,List<GameObject>>();
    }

    //初始化
    void Start(){
        ParticleInit();
    }

    //特效初始化
    public void ParticleInit()
    {
        for(int i = 0;i < particlesPres.Length; ++i)
        {
            List<GameObject> temp = new List<GameObject>();
            for(int j = 0;j < num; ++j)
            {
                GameObject particle = Instantiate(particlesPres[i]);
                particle.SetActive(false);
                temp.Add(particle);
            }
            particlePool.Add(temp[0].name,temp);
        }
    }


    //取特效
    public GameObject TakeParticle(string name)
    {
        // //如果现在对象池里不等于0 就取
        // if(particlePool[name].Count > 0)
        // {
        //     Debug.Log("qu");
        //     //把第0个取出来
        //     GameObject par = particlePool[name][0];
        //     //移除
        //     particlePool[name].Remove(par);
        //     //激活
        //     par.SetActive(true);
        //     return par;
        // }
        // //如果现在对象池里面没有特效就不取
        // else if(particlePool[name].Count == 0)
        // {
        //     Debug.Log("meile");
        //     return null;
        // }
        for(int i=0;i<particlePool[name].Count;i++){
            //如果没激活就激活
            if(!particlePool[name][i].activeSelf)
            {
                GameObject temp = particlePool[name][i];
                temp.SetActive(true);
                return temp;
            }
        }
        Debug.Log("123");
        return null;
    }


    // //放回特效
    // public void PutParticle(string name,GameObject par)
    // {
    //     //如果存在 加入
    //     if(particlePool[name] != null)
    //     {
    //         //加入 不激活
    //         par.SetActive(false);
    //         particlePool[name].Add(par);
    //     }
    //     //不存在就取消
    //     else
    //     {
    //         Debug.Log("不存在");
    //         return ;
    //     }
    // }
    

}
