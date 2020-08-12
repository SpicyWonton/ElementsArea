using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum enemyStatue { idle,move,attack};
    public enemyStatue states = enemyStatue.idle;
    public GameObject targetObj;
    public float maxHoundDistance = 15f;//最大追击距离
    public float maxAttackDistance = 1f;//最大攻击距离
    public float moveSpeed = 10f;//移动速度
    public float attackSpeed = 1f;//攻击速度 一分钟攻击一次
    public float attackForce = 5f;
    public GameObject weaponCube;
    public bool isAttack = true;//是否会攻击

    [SerializeField]
    private bool banMove = false;//禁止移动
    private Transform gunTran;
    private float currentAttackTime = 0f;//当前攻击时间
    private Animator anim;

    

    // Start is called before the first frame update
    void Start()
    {
        gunTran = transform.Find("GunPos").GetComponent<Transform>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        currentAttackTime += Time.deltaTime;
        switch(states)
        {
            case enemyStatue.idle:IdleState();
                break;
            case enemyStatue.move:MoveState();
                break;
            case enemyStatue.attack:AttackState();
                break;
        }
    }
    private void IdleState()
    {
        //与目标距离小于最远追击距离 进入移动状态
        if (Vector3.Distance(transform.position, targetObj.transform.position) <= maxHoundDistance)
            states = enemyStatue.move;
        anim.SetFloat("Speed", 0f);
    }
    private void MoveState()
    {
        //与目标距离大于最远追击距离 进入待机状态
        if (Vector3.Distance(transform.position, targetObj.transform.position) > maxHoundDistance)
            states = enemyStatue.idle;
        //与目标距离小于等于最大攻击距离 进入攻击状态
        if (Vector3.Distance(transform.position, targetObj.transform.position) <= maxAttackDistance)
            states = enemyStatue.attack;

        if(!banMove)
        {
            transform.LookAt(targetObj.transform);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            anim.SetFloat("Speed", 1f);
        }
    }
    private void AttackState()
    {
        //与目标距离大于最大攻击距离 进入攻击状态
        if (Vector3.Distance(transform.position, targetObj.transform.position) > maxAttackDistance)
            states = enemyStatue.move;

        if (currentAttackTime >= attackSpeed && isAttack)
        {
            currentAttackTime = 0f;
            GameObject tempObj = Instantiate(weaponCube, gunTran);
            tempObj.transform.position = gunTran.position;
            tempObj.GetComponent<Transform>().SetParent(null);
            tempObj.GetComponent<Rigidbody>().AddForce(transform.forward * attackForce, ForceMode.Impulse);
            Debug.Log(this.name + " attack");
        }
        anim.SetFloat("Speed", 0f);
    }
    public void SetBanMove(bool isBanMove)
    {
        banMove = isBanMove;
    }
}
