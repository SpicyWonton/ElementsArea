using GrpcLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using System;

public class PlayerControl : MonoBehaviour
{
    [Header("===== 基础属性 =====")]
    public float maxMoveSpeed;//最大移动速度 不要改
    public float moveSpeed;//移动速度
    public float turnSpeed;//转向速度
    public GameObject model;//模型
    public float attackSpeed = 1f;//攻击速度
    public float maxAttackForce;//最大攻击力量(距离
    public float sprintForce = 10f;//冲刺力度
    public GameObject commonCube;//周围没有的情况下举起什么
    public bool banMove = false;//禁止移动
    public bool banAttack = false;//禁止攻击
    public bool banTake = false;//禁止拿取
    public bool haveInertia = false;
    public AudioClip runAudio;//跑步音效
    public AudioSource audioSource;//声音播放器

    [Header("===== 其他设定 =====")]
    public bool isLW = false;//是否是联机模式
    public bool isMy = false;//当前玩家是不是我

    [SerializeField]
    public float targetSpeed = 0f;
    private float currSpeed = 0f;
    private Transform gunTran;//枪的位置
    private Animator anim;//动画状态机
    private float currentAttackTime = 0f;//当前攻击时间
    private float moveX, moveY;//移动的x，y坐标量
    private Vector3 tempVec;//临时保存移动
    public bool haveWeapon = false;//目前是否举有武器
    private GameObject weaponCube;//当前举起的东西
    private PlayerManager pm;//获取PlayerManager

    [Header("移动端控制")]
    public GameObject joystick;     // 移动的虚拟摇杆

    [Header("网络相关")]
    public Vector3 targetPos;   //用于插值的位置
    public bool canLerp = false;//是否可插值

    //private int num = 1;            //测试用的

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponentInChildren<Animator>();
        gunTran = transform.Find("GunPos");

        pm = GetComponent<PlayerManager>();

        targetPos = transform.position;
    }

    private void Start()
    {
        maxMoveSpeed = moveSpeed;
    }

    private void FixedUpdate()
    {
        // 如果不是自己，接收位置等信息
        if (!isMy && canLerp)
        {
            UpdateTransform();
        }
    }

    private void Update()
    {
        if (joystick != null)
        {
            moveX = joystick.GetComponent<Joystick>().Horizontal;
            moveY = joystick.GetComponent<Joystick>().Vertical;
        }

        //没有禁止移动的情况下
        if (!banMove && isMy)
        {
            MoveAndTurn(moveX, moveY);
        }
        currentAttackTime += Time.deltaTime;
        //修复weaponcube掉落
        if (weaponCube == null)
        {
            haveWeapon = false;
            //放下东西 
            if (pm.GetPlayerTake())
                pm.SetPlayerTake(false);
        }
        //移动状态机变化
        currSpeed = Mathf.Lerp(currSpeed, targetSpeed, 0.1f);
        anim.SetFloat("Speed", currSpeed);

        // //不是本人的时候更新
        // if (!isMy)
        //     OthersMoveAndTurn();

    }
    //移动和转向
    private void MoveAndTurn(float moveX, float moveY)
    {
        //计算出当前速度
        targetSpeed = Mathf.Sqrt(moveX * moveX + moveY * moveY);
        // if(targetSpeed>0.8f){
        //     audioSource.clip = runAudio;
        //     audioSource.Play();
        // }
        // if(targetSpeed<=0.8f){
        //     audioSource.Stop();
        //     audioSource.clip = null;
        // }
        //进行转向和移动
        Vector3 targetDirection = new Vector3(moveX, 0f, moveY);
        if (targetDirection != Vector3.zero)
        {
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * turnSpeed);
        }
        tempVec = model.transform.forward * currSpeed * moveSpeed * Time.deltaTime;
        transform.Translate(tempVec);
    }

    //得到当前速度
    public float GetCurrSpeed()
    {
        return currSpeed;
    }

    //同步位置等信息
    private void UpdateTransform()
    {
        if ((transform.position - targetPos).magnitude < 5)
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.1f);
        else
            transform.position = targetPos;
    }

    //点击按钮调用
    public void PickAndPlace()
    {
        if (!haveWeapon && !banTake)
        {
            Collider[] colliders = Physics.OverlapBox(model.transform.position + model.transform.up + model.transform.forward, new Vector3(0.5f, 1f, 0.3f), model.transform.rotation);
            foreach (var collider in colliders)
            {
                if (collider.tag == "Weaponcube" )
                {
                    //向服务器发送拾取请求
                    GrpcLibrary.Cube cube = new GrpcLibrary.Cube
                    {
                        CID = collider.GetComponent<AllCube>().cid
                    };
                    Client.SendMsg((uint)MSGID.Hwaveweapon, cube);
                    break;
                }
            }
        }
        else if (haveWeapon && !banTake)
        {
            //向服务器发送放置请求
            GrpcLibrary.Cube cube = new GrpcLibrary.Cube
            {
                CID = weaponCube.GetComponent<AllCube>().cid
            };
            Client.SendMsg((uint)MSGID.Placecube, cube);
        }
    }

    //收到消息调用
    public void Pick(int cid)
    {
        GameObject cube = CubeManager.instance.cubes[cid];
        cube.transform.position = gunTran.position;             //方块放在头顶上
        cube.GetComponent<Rigidbody>().freezeRotation = true;   //方块不能旋转
        Destroy(cube.GetComponent<Rigidbody>());                //摧毁刚体，避免碰撞
        cube.transform.SetParent(transform);                    //将人物设置为父物体
        weaponCube = cube;
        haveWeapon = true;
        weaponCube.GetComponent<AllCube>().SetplayerPM(pm);
        if (!pm.GetPlayerTake())
            pm.SetPlayerTake(true);
        anim.SetTrigger("Take");
    }

    //收到消息调用
    public void Place()
    {
        //先判断前方有没有物体 如果有就放在上面
        Collider[] colliders = Physics.OverlapBox(model.transform.position + model.transform.up + model.transform.forward, new Vector3(0.5f, 1f, 0.5f), model.transform.rotation);
        if (colliders.Length == 0)
        {
            weaponCube.transform.position = transform.position + model.transform.forward;
        }
        else if (colliders.Length != 0)
        {
            weaponCube.transform.position = transform.position + model.transform.forward + Vector3.up * 2f;
        }
        //生成rigidbody
        weaponCube.AddComponent<Rigidbody>();
        weaponCube.GetComponent<Rigidbody>().mass = 1000;
        weaponCube.transform.SetParent(null);
        weaponCube.GetComponent<Rigidbody>().freezeRotation = true;
        weaponCube.GetComponent<AllCube>().SetplayerPM(null);
        weaponCube = null;
        haveWeapon = false;
        if (pm.GetPlayerTake())
            pm.SetPlayerTake(false);
        anim.SetTrigger("Down");
    }

    //新手模式下使用，点击按钮调用
    public void TakeAction()
    {
        //没有举起东西 并且没有禁止拿取,找东西举
        if (!haveWeapon && !banTake)
        {
            Collider[] colliders = Physics.OverlapBox(model.transform.position + model.transform.up + model.transform.forward, new Vector3(0.5f, 1f, 0.3f), model.transform.rotation);

            foreach (var collider in colliders)
            {

                //先判断是不是玩家，玩家是不是队友，是不是处在昏迷状态
                if (collider.tag == "Player" && collider.GetComponent<PlayerManager>().isDizzy)
                {
                    //先设置有举起的东西 （不要让后面从地上捡起来
                    GameObject tempObj = collider.gameObject;
                    //设置成被举起状态 设置父物体 改变位置到举起的位置 禁用物理学旋转 
                    tempObj.GetComponent<PlayerManager>().SetIsLifted(true);
                    tempObj.transform.SetParent(transform);
                    tempObj.GetComponent<Transform>().position = gunTran.position;
                    tempObj.GetComponent<Rigidbody>().freezeRotation = true;
                    weaponCube = tempObj;
                    haveWeapon = true;
                    break;
                }
                //如果是武器方块
                else if (collider.tag == "Weaponcube")
                {
                    //生成新的方块体
                    GameObject tempObj = Instantiate(collider.gameObject);
                    //设置方块位置,禁用物理学旋转 
                    tempObj.GetComponent<Transform>().position = gunTran.position;
                    tempObj.GetComponent<Rigidbody>().freezeRotation = true;
                    //清楚物体的rigidbody
                    Destroy(tempObj.GetComponent<Rigidbody>());
                    //设置方块父物体
                    tempObj.transform.SetParent(transform);
                    //当前操作方块赋予，销毁之前方块，设置当前有武器
                    weaponCube = tempObj;
                    Destroy(collider.gameObject);
                    haveWeapon = true;
                    //将方块的持有玩家设为此
                    weaponCube.GetComponent<AllCube>().SetplayerPM(pm);
                    //举起东西 触发举起状态 
                    if (!pm.GetPlayerTake())
                        pm.SetPlayerTake(true);

                    //设置动画状态机
                    anim.SetTrigger("Take");
                    break;
                }
            }
            ////周围没有可以捡起的东西
            //if (!haveColl)
            //{
            //    //生成新的方块
            //    GameObject tempObj = Instantiate(commonCube);
            //    //设置方块位置,禁用物理学旋转 
            //    tempObj.GetComponent<Transform>().position = gunTran.position;
            //    tempObj.GetComponent<Rigidbody>().freezeRotation = true;
            //    //设置方块父物体
            //    tempObj.transform.SetParent(transform);
            //    //清楚物体的rigidbody
            //    Destroy(tempObj.GetComponent<Rigidbody>());
            //    //当前操作方块赋予，销毁之前方块，设置当前有武器
            //    weaponCube = tempObj;
            //    //设置成有武器
            //    haveWeapon = true;
            //    //将方块的持有玩家设为此
            //    weaponCube.GetComponent<AllCube>().SetplayerPM(pm);
            //}

            ////举起东西 触发举起状态 
            //if (!pm.GetPlayerTake())
            //    pm.SetPlayerTake(true);

            ////设置动画状态机
            //anim.SetTrigger("Take");
        }
        //有东西 并且没有禁止拿取 放下
        else if (haveWeapon && !banTake)
        {
            //先判断前方有没有物体 如果有就放在上面
            Collider[] colliders = Physics.OverlapBox(model.transform.position + model.transform.up + model.transform.forward, new Vector3(0.5f, 1f, 0.5f), model.transform.rotation);
            //前面没有物体
            if (colliders.Length == 0)
            {
                //把方块放到前面 设置父物体为空 开启物理学旋转
                weaponCube.transform.position = transform.position + model.transform.forward;
            }
            //前方有物体 放高一点
            if (colliders.Length != 0)
            {
                weaponCube.transform.position = transform.position + model.transform.forward + Vector3.up * 2f;
            }
            //生成rigidbody
            weaponCube.AddComponent<Rigidbody>();
            weaponCube.GetComponent<Rigidbody>().mass = 1000;

            weaponCube.transform.SetParent(null);
            weaponCube.GetComponent<Rigidbody>().freezeRotation = true;
            //将方块的持有玩家清空
            weaponCube.GetComponent<AllCube>().SetplayerPM(null);
            //当前武器方块清空
            weaponCube = null;
            //当前没有武器
            haveWeapon = false;

            //放下东西 
            if (pm.GetPlayerTake())
                pm.SetPlayerTake(false);

            //设置动画状态机
            anim.SetTrigger("Down");
        }
    }

    //攻击动作
    public void AttackAction(Vector3 dir)
    {
        //已经举起了东西, 并且没有禁止攻击 就进行攻击
        if (haveWeapon && !banAttack)
        {
            //如果当前拿的是cube
            if (weaponCube.CompareTag("Weaponcube"))
            {
                //生成rigidbody
                weaponCube.AddComponent<Rigidbody>();
                weaponCube.GetComponent<Rigidbody>().mass = 1000;
                //减少误差？
                Vector3 rDir = FixPoint.Round(dir);
                float rMaxAttackForce = FixPoint.Round(maxAttackForce);

                weaponCube.GetComponent<Rigidbody>().AddForce(rDir * rMaxAttackForce, ForceMode.Impulse);
                weaponCube.GetComponent<Cube>().SetOpenDamage(true);//开启伤害
            }
            //设置父物体为空 物理学旋转打开 
            weaponCube.transform.SetParent(null);
            weaponCube.GetComponent<Rigidbody>().freezeRotation = false;
            //清空当前方块 当前没有武器 
            weaponCube = null;
            haveWeapon = false;

            //设置动画状态机
            anim.SetTrigger("Throw");
        }
    }

    // 向服务器发送你所控制Player的属性
    public void SendTransform()
    {
        //定义所需内容
        Vector position = new Vector
        {
            X = transform.position.x,
            Y = transform.position.y,
            Z = transform.position.z
        };
        float tempY = model.transform.eulerAngles.y <= 180 ? model.transform.eulerAngles.y : model.transform.eulerAngles.y - 360;
        Vector rotation = new Vector
        {
            X = model.transform.eulerAngles.x,
            Y = tempY,
            Z = model.transform.eulerAngles.z
        };
        Player player = new Player
        {
            Pos = position,
            Rot = rotation,
            Speed = targetSpeed,
            PID = User.pid,
            HP = pm.currentHp
        };
        // 发送
        Client.SendMsg((uint)MSGID.Syncphv, player);
    }
}