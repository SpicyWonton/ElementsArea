using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cube : MonoBehaviour
{
    public float damage;              // 方块伤害
    public AudioClip audioClip;       // 砸中播放的音效
    public cubeType cubeWeapType;     // 砖块属性 类型
    public bool openDamage = false;   // 是否开启伤害  当玩家开始投掷之后才开启伤害

    public enum cubeType          //武器的类型
    {
        Terra,                    //土属性
        Wood,                     //爆炸范围伤害
        Water,                    //在地面形成一摊物质
        Ice,                      //冰
        Fire,                     //火
        Bomb,                     //炸弹
        Thunder                   //雷
    }

    protected Rigidbody rb;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    protected virtual void OnCollisionEnter(Collision collision) {}
    
    //玩家类调用
    public void SetOpenDamage(bool open)
    {
        openDamage = open;
    }
}
