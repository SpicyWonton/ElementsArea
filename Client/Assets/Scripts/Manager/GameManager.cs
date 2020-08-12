using GrpcLibrary;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : BaseManager<GameManager>
{
    public List<GameObject> players;          //场景中所有的玩家
    public float timeLeft;

    private int myIndex = -1;         //你所控制玩家的下标

    [Header("网络相关")]
    public float accumulatedTime = 0f;  //累积时间
    public float frameLength = 0.066f;  //逻辑帧长度
    public int frameNum = 0;            //当前逻辑帧个数
    public bool canSend = true;         //是否能发送

    public enum KillType { Hit, Burn }; //击杀的类型

    [Header("===== 道具项 =====")]
    public GameObject propsObject;
    public GameObject createPos;
    public float createTime = 30f;
    private float currCreateTime = 0f;

    protected override void Init()
    {
        ChoosePlayer();
        timeLeft = User.gameTime;
    }

    private void Start()
    {
        UIManager.UpdatePoints();    // 开局时初始化分数
    }

    private void Update()
    {
        UpdateProps();
        UpdateTime();
        UpdateFrame();
    }

    // 每个玩家根据传过来的ID选择Player
    private void ChoosePlayer()
    {
        //你所控制的Player下标
        myIndex = User.pid;
        players[myIndex].GetComponent<PlayerControl>().isMy = true;
        //给所有玩家id赋值
        for (int i = 0; i < RoomPlayerManager.Instance.roomPlayers.Count; i++)
        {
            players[i].GetComponent<PlayerManager>().myPID = i;
            players[i].GetComponent<PlayerManager>().myUID = RoomPlayerManager.Instance.GetUID(i);
            players[i].GetComponentInChildren<Text>().text = RoomPlayerManager.Instance.GetName(players[i].GetComponent<PlayerManager>().myUID);
        }
        //摧毁多余的预制体
        for (int i = RoomPlayerManager.Instance.roomPlayers.Count; i < players.Count; i++)
        {
            Destroy(players[i].gameObject);
        }
        // 相机跟随指定的玩家
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        camera.GetComponent<CameraControl>().hero = players[myIndex].transform;
        // 投掷摇杆生成的曲线跟随指定玩家
        GameObject throwJoy = GameObject.Find("Throw Joystick");
        throwJoy.GetComponent<ThrowJoystick>().playerTrans = players[myIndex].transform;
        // 设置指定玩家的移动摇杆
        GameObject moveJoy = GameObject.Find("Move Joystick");
        players[myIndex].GetComponent<PlayerControl>().joystick = moveJoy;
        // 设置指定玩家的拾取按钮
        GameObject pickBtn = GameObject.Find("Pick Button");
        pickBtn.GetComponent<Button>().onClick.AddListener(delegate { players[myIndex].GetComponent<PlayerControl>().PickAndPlace(); });
        // 设置指定本人的音频播放
        GameObject audioBtn = GameObject.Find("Audio Button");
        audioBtn.GetComponent<VoiceAudio>().audioSource = players[myIndex].GetComponent<PlayerManager>().voicePos.GetComponent<AudioSource>();

    }

    //更新游戏时间
    private void UpdateTime()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
            timeLeft = 0;
        UIManager.UpdateGameTime(timeLeft);
    }

    //更新逻辑帧
    private void UpdateFrame()
    {
        accumulatedTime += Time.deltaTime;
        while (accumulatedTime > frameLength)
        {
            frameNum++;
            if (canSend)
            {
                players[myIndex].transform.GetComponent<PlayerControl>().SendTransform();
                canSend = false;
            }
            accumulatedTime -= frameLength;
        }
    }

    //玩家类击败敌人/拾取加分道具时调用，用于更新分数
    public static void AddPoints(string uid, int point)
    {
        //向服务器发送消息，更新服务器中的分数信息
        PlayerPoint msg = new PlayerPoint
        { 
            UID = uid,
            Point = point 
        };
        Client.SendMsg((uint)MSGID.Addscore, msg);
    }

    //收到消息后调用
    public static void UpdatePoints(string uid, int points)
    {
        //更新分数
        foreach (var roomPlayer in RoomPlayerManager.Instance.roomPlayers)
        {
            if (roomPlayer.uid == uid)
            {
                roomPlayer.points = points;
                break;
            }    
        }
        //排序
        RoomPlayerManager.Instance.roomPlayers.Sort();
        UIManager.UpdatePoints();
    }

    //显示击杀播报
    public static void UpdateKillRadio(string uid1, string uid2, KillType type)
    {
        string name1 = RoomPlayerManager.Instance.GetName(uid1);
        string name2 = RoomPlayerManager.Instance.GetName(uid2);

        UIManager.KillType killType = UIManager.KillType.OHO;
        if (type == KillType.Hit)
        {
            if (User.uid == uid1)
                killType = UIManager.KillType.MHO;
            else if (User.uid == uid2)
                killType = UIManager.KillType.OHM;
            else
                killType = UIManager.KillType.OHO;
        }
        else if (type == KillType.Burn)
        {
            if (User.uid == uid1)
                killType = UIManager.KillType.MBO;
            else if (User.uid == uid2)
                killType = UIManager.KillType.OBM;
            else
                killType = UIManager.KillType.OBO;
        }

        UIManager.UpdateKillRadio(name1, killType, name2);
    }
    //更新道具
    public void UpdateProps()
    {
        currCreateTime += Time.deltaTime;
        if(currCreateTime >= createTime)
        {
            currCreateTime = 0f;
            GameObject props = Instantiate(propsObject);
            props.transform.position = createPos.transform.position;
        }
    }
}
