using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GrpcLibrary;
using Google.Protobuf;

public class PlayerManager : MonoBehaviour
{
    [Header("网络相关")]
    public string myUID;   //我的uid
    public int myPID;      //我的pid
    private System.Random random;

    [Header("UI")]
    public GameObject addPoint;     //加分
    public float showTime;          //加分UI显示时间
    private float currTime;         

    [Header("===== 基础属性 =====")]
    public float maxHp;//最大血量
    public float currentHp;//当前血量
    public float range;//攻击范围
    public float defense;//防御力
    public bool isAI = false;//是否是AI
    public GameObject aiTargetPos;//AI的目标位置
    public ParticleSystem revieParticle;//复活特效
    public AudioClip reviewAudio;//复活音效
    [SerializeField]
    private HpFollow hpControl;//血条控制
    private PlayerControl pc;//获取玩家控制脚本
    private ParticleSystem currentRevieParticle;//当前复活特效
    private int currentRevieNum = 0;//当前复活位置
    

    [Header("===== 所需组件 =====")]
    public Animator anim;//动画状态机
    public AudioSource audioSource;//声音播放器
    public GameObject[] rebirthPoints;//复活点位置
    public GameObject voicePos;//声音播放位置
    private Transform particlePos;//特效位置
    private GameObject wakeParticle;//尾迹特效
    [SerializeField]
    private GameObject audioImg = null;//音响图标

    public float wakeTime;//苏醒需要的时间

    [Header("===== 玩家状态 =====")]
    public bool isDizzy = false;//是否处于昏迷状态
    public bool isLifted = false;//是否是被举起状态
    public bool isFire = false;//是否处在燃烧状态
    public bool isRes = false;//是否处在生机状态(持续回血
    public bool isWet = false;//是否是潮湿状态(移动速度增加
    public bool isIce = false;//是否处于冰冻状态(移动速度减慢
    public bool isSlider = false;//是否是滑行状态
    public bool isTake = false;//是否是搬运状态
    public bool isBorze = false;//是否是冻结状态
    public bool isDied = false;//是否死了
    public bool isBreath = false;//是否处于呼吸回血状态
    public bool isParalysis = false;//是否是麻痹状态
    public bool isSomke = false;//是否是烟雾状态

    [Header("===== 状态所需变量 =====")]
    public StaFire staFire;//着火状态
    public StaRes staRes;//复苏状态
    public StaWet staWet;//潮湿状态
    public StaIce staIce;//冰冻状态
    public StaSlide staSlider;//滑行状态
    public StaDizzy staDizzy;//眩晕状态
    public StaTake staTake;//拿取状态
    public StaBorze staBorze;//冻结状态 
    public StaBreathHp staBreath;//呼吸回血状态
    public StaParalysis staParalysis;//麻痹状态
    public StaSomke staSomke;//烟雾状态


    //燃烧状态
    [System.Serializable]
    public class StaFire
    {
        public float allFireTime = 5f;//着火持续多久
        public float Damage = 2f;//着火1s掉多少血
        public ParticleSystem fireParticle;//火焰特效
        public ParticleSystem currentParticle;//当前特效
        public float currentFireTime;//当前着火时间
        public float damageMul = 1f;//伤害倍率
        public GameObject attackSource = null;//攻击来源

    }
    [System.Serializable]
    //生机状态
    public class StaRes
    {
        public float allResTime = 5f;//生机持续时间
        public float Damage = 2f;//生机1s掉多少血
        public ParticleSystem ResParticle;//生机特效
        public ParticleSystem currentParticle;//当前特效
        public float currentResTime;//当前复苏时间
    }
    [System.Serializable]
    //潮湿状态
    public class StaWet
    {
        public float allWetTime = 5f;//潮湿持续时间
        public float Value = 2f;//潮湿状态的速度
        public float Mul = 1f;//潮湿状态影响倍率
        public ParticleSystem WetParticle;//潮湿特效
        public ParticleSystem currentParticle;//当前特效
        public float currentWetTime;//当前潮湿时间
    }

    [System.Serializable]
    //冰冻状态
    public class StaIce
    {
        public float allIceTime = 5f;//冰冻持续时间
        public float Value = 2f;//冰冻状态的速度
        public ParticleSystem IceParticle;//冰冻特效
        public ParticleSystem currentParticle;//当前特效
        public float currentIceTime;//当前冰冻时间
    }

    [System.Serializable]
    //滑行状态
    public class StaSlide
    {
        public float allSliderTime = 5f;//滑行CD
        public float currentSliderTime;//当前冰冻时间
        public float sliderForece = 5f;//打滑的力度大小
    }

    [System.Serializable]
    //眩晕状态
    public class StaDizzy
    {
        public float allTime = 5f;//总时间
        public ParticleSystem particle;//眩晕特效
        public ParticleSystem currentParticle;//当前特效
        public float currentTime = 0f;//当前时间
    }

    [System.Serializable]
    //搬运状态 减速
    public class StaTake
    {
        public bool isEffect = true;//是否是有效的
        public float Value = 3.5f;//搬运状态的速度
    }
    
    [System.Serializable]
    //冻结状态 禁止移动
    public class StaBorze
    {
        public float allTime = 2f;//冻结时间
        public float currentTime = 0f;//当前冻结时间
        public ParticleSystem particle;//冻结特效
        public ParticleSystem currentParticle;//当前特效
    }
    [System.Serializable]
    //麻痹状态 禁止移动 攻击 拿取 间歇性一段时间一次
    public class StaParalysis
    {
        public float allTime = 2f;//麻痹总时间
        public float currentTime = 0f;//当前麻痹时间
        public float interTime = 0.3f;//多久麻痹一次
        public float effTime = 0.2f;//麻痹一次麻痹多少秒
        public ParticleSystem particle;//麻痹特效
        public ParticleSystem currentParticle;//当前特效
        public bool particleEff = false;//特效生效了吗
        public float mul = 1f;//效果倍率
    }
    [System.Serializable]
    //呼吸回血
    public class StaBreathHp
    {
        public float startTime = 5f;//多久没有受到攻击回血
        public float currTime = 0f;//当前时间
        public float value = 5f;//每秒恢复血量
    }
    [System.Serializable]
    //烟雾状态
    public class StaSomke
    {
        public float allTime = 3f;//持续时间
        public float currentTime = 0f;//当前时间
        public float mul = 1f;//时间倍率
        public GameObject blockPanel;//烟雾UI
    }

    void Awake()
    {
        //初始化UI显示时间
        currTime = showTime;
        //初始化随机数
        random = new System.Random(User.seed1);
        //如果是AI把pid设为100
        if(isAI)
            myPID = 100;
    }
    void Start()
    {
        rebirthPoints = GameObject.FindGameObjectsWithTag("Rebirthpoint");
        wakeParticle = transform.Find("WalkParticle").gameObject;
        particlePos = transform.Find("ParticlePos");
        currentHp = maxHp;
        anim = GetComponentInChildren<Animator>();
        hpControl = GetComponentInChildren<HpFollow>();
        pc = GetComponent<PlayerControl>();
        audioSource = GetComponent<AudioSource>();
        //staSomke.blockPanel = GameObject.Find("Canvas/BlockPanel");

        //生成特效


        //生成麻痹特效 设置位置
        staParalysis.currentParticle = Instantiate(staParalysis.particle);
        staParalysis.currentParticle.transform.position = particlePos.position + Vector3.up;
        staParalysis.currentParticle.transform.SetParent(transform);
        staParalysis.currentParticle.gameObject.SetActive(false);
        //生成火焰特效 设置位置
        staFire.currentParticle = Instantiate(staFire.fireParticle);
        staFire.currentParticle.transform.position = particlePos.position + Vector3.up;
        staFire.currentParticle.transform.SetParent(transform);
        staFire.currentParticle.gameObject.SetActive(false);

        //生成潮湿特效 设置位置
        staWet.currentParticle = Instantiate(staWet.WetParticle);
        staWet.currentParticle.transform.position = particlePos.position + Vector3.up;
        staWet.currentParticle.transform.SetParent(transform);
        staWet.currentParticle.gameObject.SetActive(false);
    }
    
    void Update()
    {
        //显示加分UI
        if (!isAI && addPoint.activeSelf)
        {
            currTime -= Time.deltaTime;
            if (currTime <= 0)
            {
                currTime = showTime;
                addPoint.SetActive(false);
            }
        }

        //如果是AI
        if(isAI)
        {
            //如果不在禁止移动的情况下
            if(!GetPlayerBanMove())
            {
                transform.Translate(Vector3.forward * pc.moveSpeed * Time.deltaTime);
                transform.LookAt(aiTargetPos.transform);
                anim.SetFloat("Speed", 0.5f);
            }
        }

        //设置尾迹特效
        if (pc.GetCurrSpeed()<0.05f)
        {
            wakeParticle.SetActive(false);
        }
        if (pc.GetCurrSpeed()>=0.05f)
        {
            wakeParticle.SetActive(true);
        }

        //如果是着火状态
        if (isFire)
        {
            StateFire();
            staFire.currentFireTime += Time.deltaTime;
        }

        //如果是复苏状态
        //if(isRes)
        //{
        //    StateRes();
        //    staRes.currentResTime += Time.deltaTime;
        //}

        //如果是潮湿状态
        if (isWet)
        {
            StateWet();
            staWet.currentWetTime += Time.deltaTime;
        }
        
        //如果是冰冻状态
        //if(isIce)
        //{
        //    StateIce();
        //    staIce.currentIceTime += Time.deltaTime;
        //}

        //滑行状态
        if (isSlider)
        {
            GetComponent<Rigidbody>().AddForce(pc.model.transform.forward*staSlider.sliderForece,ForceMode.Impulse);
            staSlider.currentSliderTime += Time.deltaTime;
            //当前滑行时间大于 最大滑行时间
            if (staSlider.currentSliderTime > staSlider.allSliderTime)
            {
                staSlider.currentSliderTime = 0f;
                isSlider = false;
            }
        }

        //冻结状态
        if (isBorze)
        {
            StateBorze();
            staBorze.currentTime += Time.deltaTime;
        }

        //呼吸回血状态
        if (isBreath && currentHp != maxHp)
        {
            StateBreath();
            staBreath.currTime +=Time.deltaTime;
        }

        //麻痹状态
        if (isParalysis)
        {
            StateParalysis();
            staParalysis.currentTime += Time.deltaTime;
        }

        //烟雾状态
        if (isSomke)
        {
            StateSomke();
            staSomke.currentTime += Time.deltaTime;
        }
    }

    //血量加减都调用这个
    public void AddHp(float damage)
    {
        //如果不是AI
        if(!isAI)
        {
            //被攻击的时候 呼吸回血时间重置为0
            if(damage < 0)
                staBreath.currTime = 0f;
            //控制血量在0和最大值之间
            currentHp = Mathf.Clamp(currentHp + damage, 0, maxHp);
            //更新血条
            hpControl.UpdateHealth();
            //如果血量小于等于0 死亡
            if (currentHp <= 0.0f)
                Die();
        }
        //如果是AI的情况
        else if(isAI)
        {
            //被攻击的时候 呼吸回血时间重置为0
            if(damage < 0)
                staBreath.currTime = 0f;
            //控制血量在0和最大值之间
            currentHp = Mathf.Clamp(currentHp + damage, 0, maxHp);
            //更新血条
            hpControl.UpdateHealth();
            //如果血量小于等于0 回血
            if (currentHp <= 0.0f)
                AddHp(1000);
        }
    }

    //角色死亡
    public void Die()
    {
        //死亡只执行一次
        if (!isDied)
        {
            isDied = true;

            //如果被烧死算扔火的人
            if (staFire.attackSource != null)
            {
                PlayerManager addScorePm = staFire.attackSource.GetComponent<PlayerManager>();
                //加分
                string uid1 = addScorePm.myUID;
                string uid2 = myUID;
                GameManager.UpdateKillRadio(uid1, uid2, GameManager.KillType.Burn);
                //加分UI
                UIManager.UpdateAddPoint(addScorePm, 2);
                //计分
                if (uid1 != uid2)
                    addScorePm.GetSocre(2);
            }
            
            //随机选择一个复活点
            currentRevieNum = random.Next(0, rebirthPoints.Length); //复活点序号

            //镜头控制
            if (pc.isMy)
                Camera.main.GetComponent<CameraControl>().hero = rebirthPoints[currentRevieNum].transform;
            //人物控制
            transform.position += -transform.up * 50f;
            //当前复活特效不为空
            if (currentRevieParticle == null)
            {
                Vector3 tempPos = rebirthPoints[currentRevieNum].transform.position;
                currentRevieParticle = Instantiate(revieParticle);
                currentRevieParticle.transform.position = new Vector3(tempPos.x, 0.5f, tempPos.z);
                currentRevieParticle.Stop();
            }
            currentRevieParticle.Play();
            //播放音效
            audioSource.clip = reviewAudio;
            audioSource.Play();
            Invoke("Revie", 2.7f);
        }
    }

    //复活
    public void Revie()
    {
        //清空状态
        SetPlayerFire(false,null);
        SetPlayerWet(false);
        SetPlayerParalysis(false);

        isDied = false;
        AddHp(10000);
        if (pc.isMy)
            Camera.main.GetComponent<CameraControl>().hero = gameObject.transform;
        transform.position = rebirthPoints[currentRevieNum].transform.position;
    }

    //设置玩家是否被举起
    public void SetIsLifted(bool lifted)
    {
        isLifted = lifted;
    }

    //收到消息后同步血量和血条
    public void UpdateHealth(float hp)
    {
        currentHp = hp;
        hpControl.UpdateHealth();
    }

    //得分
    public void GetSocre(int score)
    {
        if (User.isHost && !User.isSingle)
            GameManager.AddPoints(myUID, score);
    }
    //===============================================更改音效====================================
    public void SetAudioClip(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    //========================================================================取得 or 改变玩家状态=================================================

    //得到玩家是否处于燃烧状态
    public bool GetPlayerFire()
    {
        return isFire;
    }
    //设置玩家是否处于燃烧状态
    public void SetPlayerFire(bool playerFire, GameObject attackObj)
    {
        isFire = playerFire;
        //每次激活的时候重置时间
        if (playerFire)
        {
            staFire.currentFireTime = 0f;
            staFire.attackSource = attackObj;
        }
        else if (!playerFire)
        {
            //如果在燃烧
            if (staFire.currentParticle.gameObject.activeInHierarchy)
            {
                staFire.currentParticle.gameObject.SetActive(false);
            }
            staFire.attackSource = null;
            staFire.damageMul = 1f;
            staFire.currentFireTime = 0f;
            isFire = false;
        }
    }
    public void SetPlayerFireDamageMul(float daMul)
    {
        staFire.damageMul = daMul;
    }
 
    
    //得到玩家是否处于生机状态
    public bool GetPlayerRes()
    {
        return isRes;
    }
    //设置玩家是否处于生机状态
    public void SetPlayerRes(bool playerRes)
    {
        isRes = playerRes;
        if(playerRes)
            staRes.currentResTime = 0f;
    }

    //得到玩家是否处于潮湿状态
    public bool GetPlayerWet()
    {
        return isWet;
    }
    //设置玩家是否处于潮湿状态
    public void SetPlayerWet(bool playerWet)
    {
        isWet = playerWet;
        //Debug.Log("Wet:"+isWet);
        if (playerWet)
            staWet.currentWetTime = 0f;
        else if(!playerWet)
        {
            //如果粒子效果还存在
            if(staWet.currentParticle.gameObject.activeInHierarchy)
            {
                staWet.currentParticle.gameObject.SetActive(false);
            }
            staWet.Mul = 1f;
            staWet.currentWetTime = 0f;
            isWet = false;
            pc.moveSpeed = pc.maxMoveSpeed;
        }
    }
    //设置潮湿状态效果倍率
    public void SetPlayerWetMul(float mul)
    {
        staWet.Mul = mul;
    }

    //得到玩家是否处于冰冻状态
    public bool GetPlayerIce()
    {
        return isIce;
    }
    //设置玩家是否处于冰冻状态
    public void SetPlayerIce(bool playerIce)
    {
        isIce = playerIce;
        if (playerIce)
            staIce.currentIceTime = 0f;
    }

    //查看玩家是否能禁止移动
    public bool GetPlayerBanMove()
    {
        return pc.banMove;
    }
    //设置玩家是否禁止移动状态
    public void SetPlayerBanMove(bool banMove)
    {
        pc.banMove = banMove;
    }
    //查看玩家是否能禁止拿取
    public bool GetPlayerBanTake()
    {
        return pc.banTake;
    }
    //设置玩家是否禁止拿取状态
    public void SetPlayerBanTake(bool banTake)
    {
        pc.banTake = banTake;
    }
    //查看玩家是否在滑行状态
    public bool GetPlayerSlider()
    {
        return isSlider;
    }
    //设置玩家是否在滑行状态
    public void SetPlayerSlider(bool slider)
    {
        isSlider = slider;
    }

    //查看玩家是否禁止攻击
    public bool GetPlayerBanAttack()
    {
        return pc.banAttack;
    }
    //设置玩家是否在滑行状态
    public void SetPlayerBanAttack(bool attack)
    {
        pc.banAttack = attack;
    }

    //查看玩家是否是眩晕状态
    public bool GetPlayerDizzy()
    {
        return isDizzy;
    }
    //设置玩家是否处于眩晕状态
    public void SetPlayerDizzy(bool dizzy)
    {
        isDizzy = dizzy;
        staDizzy.currentTime = 0f;
    }

    //查看玩家是否是搬运状态
    public bool GetPlayerTake()
    {
        return isTake;
    }
    //设置玩家是否处于搬运状态
    public void SetPlayerTake(bool take)
    {
        isTake = take;
        if(take)
        {
            pc.moveSpeed = staTake.Value;
        }
        else
        {
            pc.moveSpeed = pc.maxMoveSpeed;
        }
    }    
    //查看玩家是否是冻结状态
    public bool GetPlayerBroze()
    {
        return isBorze;
    }
    //设置玩家是否处于搬运状态
    public void SetPlayerBorze(bool borze)
    {
        isBorze = borze;        
        if (isBorze)
            staBorze.currentTime = 0f;
    }

    //查看玩家是否能麻痹
    public bool GetPlayerParalysis()
    {
        return isParalysis;
    }
    //设置玩家是否麻痹状态
    public void SetPlayerParalysis(bool Paralysis)
    {
        isParalysis = Paralysis;
        if(isParalysis)
            staParalysis.currentTime = 0f;
        else if(!isParalysis)
        {
            //清空禁止移动 攻击 搬取
            SetPlayerBanAttack(false);
            SetPlayerBanMove(false);
            SetPlayerBanTake(false);
            //如果粒子还存在
            if(staParalysis.currentParticle.gameObject.activeInHierarchy)
            {
                staParalysis.currentParticle.gameObject.SetActive(false);
            }
            staParalysis.currentTime = 0f;
            isParalysis = false;
            staParalysis.mul = 1f;
        }
    }
    //设置玩家麻痹状态系数
    public void SetPlayerParalysisMul(float mul)
    {
        staParalysis.mul = mul;
    }

    //获得玩家是否在烟雾状态
    public bool GetPlayerSmoke()
    {
        return isSomke;
    }
    //设置玩家烟雾状态
    public void SetPlayerSmoke(bool smoke)
    {
        // isSomke = smoke;
        // if(isSomke)
        //     staSomke.currentTime = 0f;
        // else if(!isSomke)
        // {
        //     staSomke.blockPanel.SetActive(false);
        //     staSomke.currentTime = 0f;
        //     staSomke.mul = 1f;
        // }
    }

    //===============================================================================玩家状态触发的函数================================================================

    //燃烧状态 逐渐减血
    private void StateFire()
    {
        if(staFire.currentFireTime == 0)
        {
            //特效产生 特效位置改变 特效设置父物体
            if (staFire.currentParticle == null)
            {
                staFire.currentParticle = Instantiate(staFire.fireParticle);
                staFire.currentParticle.transform.position = particlePos.position + Vector3.up;
                staFire.currentParticle.transform.SetParent(transform);
            }
            else if (staFire.currentParticle != null)
            {
                staFire.currentParticle.gameObject.SetActive(true);
            }
        }
        //当前着火时间小于等于着火总时间 触发伤害
        if(staFire.currentFireTime <= staFire.allFireTime)
        {
            AddHp(-staFire.Damage * Time.deltaTime * staFire.damageMul);
        }
        //当前着火时间大于着火总时间 着火状态结束
        else if(staFire.currentFireTime > staFire.allFireTime)
        {
            //特效删除
            if(staFire.currentParticle.gameObject.activeInHierarchy)
            {
                staFire.currentParticle.gameObject.SetActive(false);
            }
            //攻击来源设为false
            staFire.attackSource = null;
            //伤害倍率设置为1倍
            staFire.damageMul = 1f;
            //置空当前时间 设置火焰状态为false
            staFire.currentFireTime = 0f;
            isFire = false;

        }
    }

    //潮湿状态 玩家减速
    private void StateWet()
    {
        //第一次调用时产生特效
        if (staWet.currentWetTime == 0)
        {
            //特效产生 特效位置改变 特效设置父物体
            if (staWet.currentParticle == null)
            {
                staWet.currentParticle = Instantiate(staWet.WetParticle);
                staWet.currentParticle.transform.position = particlePos.position + Vector3.up;
                staWet.currentParticle.transform.SetParent(transform);
            }
            else if (staWet.currentParticle != null)
            {
                staWet.currentParticle.gameObject.SetActive(true);
            }
        }
        //当前潮湿时间 小于等于 总时间  并且当前速度不等于潮湿速度
        if (staWet.currentWetTime <= staWet.allWetTime && pc.moveSpeed != staWet.Value)
        {
            //将速度给玩家赋值
            pc.moveSpeed = staWet.Value * staWet.Mul;
        }
        //当前潮湿时间 大于 总时间 退出潮湿状态
        else if(staWet.currentWetTime > staWet.allWetTime && pc)
        {
            //特效删除
            if(staWet.currentParticle.gameObject.activeInHierarchy)
            {
                staWet.currentParticle.gameObject.SetActive(false);
            }
            //重置频率
            staWet.Mul = 1f;
            //置空当前时间 速度还原 设置潮湿状态为false
            pc.moveSpeed = pc.maxMoveSpeed;
            staWet.currentWetTime = 0f;
            isWet = false;
        }
    }

    //冻结状态
    private void StateBorze()
    {
       //第一次调用时产生特效 并冻结
        if (staBorze.currentTime == 0)
        {
            //特效产生 特效位置改变 特效设置父物体
            if (staBorze.currentParticle == null)
            {
                staBorze.currentParticle = Instantiate(staBorze.particle);
                staBorze.currentParticle.transform.position = particlePos.position+Vector3.up*0.25f;
                staBorze.currentParticle.transform.SetParent(transform);
            }
            else if (staBorze.currentParticle != null)
            {
                staBorze.currentParticle.gameObject.SetActive(true);
            }
            //冻结移动 和 攻击
            SetPlayerBanMove(true);
            SetPlayerBanAttack(true);
            //速度滞空
            pc.targetSpeed = 0f;
        }
        //当前冻结时间 大于 总时间 解除冻结状态
        else if (staBorze.currentTime >= staBorze.allTime)
        {
            //Debug.Log("exit");
            //特效删除
            staBorze.currentParticle.gameObject.SetActive(false);

            //置空当前时间 解除限制 设置冰冻状态为false
            SetPlayerBanMove(false);
            SetPlayerBanAttack(false);
            staBorze.currentTime = 0f;
            isBorze = false;
        }
    }

    //呼吸回血状态
    private void StateBreath()
    {
        //呼吸回血
        if(staBreath.currTime > staBreath.startTime)
        {
            //AddHp(staBreath.value * Time.deltaTime);
        }
    }

    //麻痹状态
    private void StateParalysis()
    {
        //如果当前时间 小于 总时间 执行麻痹状态
        if(staParalysis.currentTime <= staParalysis.allTime)
        {
            //一轮的时间
            float oneTime = (staParalysis.interTime + staParalysis.effTime)*staParalysis.mul;
            //如果当前时间取余 小于 有效时间 就进入麻痹状态
            if(staParalysis.currentTime % oneTime < staParalysis.effTime*staParalysis.mul)
            {
                //如果当前特效没生效就生效
                if(!staParalysis.particleEff)
                {
                    staParalysis.particleEff = true;
                    //特效生效代码
                    staParalysis.currentParticle.gameObject.SetActive(true);
                    staParalysis.currentParticle.transform.position = particlePos.position + Vector3.up;
                }

                //禁止攻击移动拿取
                //SetPlayerBanAttack(true);
                //SetPlayerBanTake(true);
                SetPlayerBanMove(true);
            }
            //如果当前时间取余 大于等于 有效时间 退出麻痹状态
            else if(staParalysis.currentTime % oneTime >= staParalysis.effTime*staParalysis.mul)
            {
                //特效生效取消
                staParalysis.particleEff = false;
                SetPlayerBanAttack(false);
                SetPlayerBanMove(false);
                SetPlayerBanTake(false);
                //特效取消 如果特效还存在就取消
                if(staParalysis.currentParticle.gameObject.activeInHierarchy)
                {
                    staParalysis.currentParticle.gameObject.SetActive(false);
                }
            }
        }
        //如果当前时间 大于 总时间
        else
        {
            staParalysis.mul = 1f;
            isParalysis = false;
            staParalysis.currentTime = 0f;
        }
    }

    //烟雾状态
    private void StateSomke()
    {
        if(staSomke.currentTime == 0)
        {
            staSomke.blockPanel.SetActive(true);
            // Color tempCol = staSomke.blockPanel.GetComponent<Image>().color;
            // tempCol.a = 150f;
            // staSomke.blockPanel.GetComponent<Image>().color = new Color(tempCol.r,tempCol.g,tempCol.b,tempCol.a/255f);
        }
        //当前时间大于总时间 结束烟雾状态
        else if(staSomke.currentTime > staSomke.allTime)
        {
            //UI还原
            // Color tempCol = staSomke.blockPanel.GetComponent<Image>().color;
            // tempCol.a = 0f;
            // staSomke.blockPanel.GetComponent<Image>().color = new Color(tempCol.r,tempCol.g,tempCol.b,tempCol.a/255f);
            staSomke.blockPanel.SetActive(false);
            //状态结束
            isSomke = false;
            staSomke.currentTime = 0f;
        }
    }


    public void SetAudioImg(bool isOpen,float clipTime)
    {
        audioImg.SetActive(isOpen);
        Invoke("EndAudioImg",clipTime);
    }
    private void EndAudioImg()
    {
        audioImg.SetActive(false);
    }
}
