using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTerrain : MonoBehaviour
{
    [Header("===== 火草伤害属性 =====")]
    public float damage = 5f;//伤害大小
    public float damageTime = 1f;//伤害频率 1s1次
    protected float currTime = 0f;

    [Header("===== 水属性 =====")]
    public float waterMoveSpeed;//水属性移动的速度
    public float addInertia;//变化后额外加的惯性

    [Header("===== 冰属性 =====")]
    public float iceMoveSpeed;//冰属性移动的速度


    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Update()
    {
        currTime += Time.deltaTime;
    }
}
