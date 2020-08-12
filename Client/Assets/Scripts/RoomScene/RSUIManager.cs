using Google.Protobuf.Collections;
using GrpcLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RSUIManager : BaseManager<RSUIManager>
{
    [Header("需要显示和关闭的面板")]
    public GameObject outsideBG;    // 显示房间列表的页面
    public GameObject insideBG;     // 显示房间信息的页面
    public GameObject loadBG;       // 加载进度的页面
    public GameObject createPanel;  // 创建房间的面板
    public GameObject warnPanel;    // 提示房间号错误的面板

    [Header("按钮")]
    public Button refreshRoom;      // 刷新房间按钮
    public Button createRoom;       // 创建房间按钮
    public Button enterRoom;        // 进入房间按钮
    public Button backToMS;         // 返回主页面
    public Button createBtn;        // 创建按钮
    public Button cancelBtn;        // 取消按钮
    public Button confirmBtn;       // 确定按钮
    public Button exitBtn;          // 离开房间按钮

    [Header("房间列表")]
    public GameObject roomInfo;                                 // 房间信息预制体
    public List<GameObject> roomList = new List<GameObject>();  // 当前显示在UI上的房间列表
    public Transform content;                                   // 房间信息的父物体

    //房间内玩家信息
    private class RoomPlayer
    {
        public string uid;
        public string name;
        public bool isReady;
        public bool isOwner;

        public RoomPlayer(string uid, string name, bool isReady, bool isOwner)
        {
            this.uid = uid;
            this.name = name;
            this.isReady = isReady;
            this.isOwner = isOwner;
        }
    }
    private List<RoomPlayer> roomPlayers = new List<RoomPlayer>();

    [Header("房间内玩家列表")]
    public GameObject playerInfo0;                               // 玩家信息预制体-冰
    public GameObject playerInfo1;                               // 玩家信息预制体-草
    public GameObject playerInfo2;                               // 玩家信息预制体-火
    public GameObject playerInfo3;                               // 玩家信息预制体-水
    public List<GameObject> playerList = new List<GameObject>();// 当前显示在UI上的玩家列表
    public Transform playerParent;                              // 玩家信息的父物体

    [Header("准备相关")]
    public Sprite readySprite;
    public Sprite isReadySprite;
    public Sprite startSprite;

    [Header("杂项")]
    public InputField input;        // 输入框

    // 警告面板显示的错误类型
    public enum Wrong { Length, Digit, Repeate, EnterFail, NotAllReady };

    [SerializeField] private int currRoomID;          // 当前点击的房间ID
    [SerializeField] private Wrong wrong;             // 当前错误类型

    protected override void Init()
    {
        refreshRoom.onClick.AddListener(SendRefreshReq);
        createRoom.onClick.AddListener(ShowCreatePanel);
        enterRoom.onClick.AddListener(SendEnterRoom);
        backToMS.onClick.AddListener(BackToMS);
        createBtn.onClick.AddListener(SendCreateRoom);
        cancelBtn.onClick.AddListener(CloseCreatePanel);
        confirmBtn.onClick.AddListener(CloseWarnPanel);
        exitBtn.onClick.AddListener(SendExitRoom);
        // 进入该场景时，先刷新一次
        SendRefreshReq();
    }

    // 点击返回主页面按钮时调用
    private void BackToMS()
    {
        SceneManager.LoadScene("MainScene");
    }

    // 点击刷新房间时向服务器发送请求
    private void SendRefreshReq()
    {
        Sync req = new Sync { Tag = true };
        Client.SendMsg((uint)MSGID.Roominfo, req);
    }

    // 点击创建时向服务器发送房间信息
    private void SendCreateRoom()
    {
        // 获取输入框的文本
        string str = input.text;
        // 房间号长度不能大于6
        if (str.Length > 6)
        {
            ShowWarnPanel(Wrong.Length);
            return;
        }
        // 如果房间号不全是数字
        if (!Regex.IsMatch(str, @"^\d+$"))
        {
            ShowWarnPanel(Wrong.Digit);
            return;
        }
        // 如果房间号格式正确，就发送给服务器
        int rid = int.Parse(str);
        Room room = new Room { Rid = rid };
        Client.SendMsg((uint)MSGID.Createroom, room);
    }

    // 点击进入房间时调用，向服务器发送房间信息
    private void SendEnterRoom()
    {
        Room room = new Room { Rid = instance.currRoomID };
        Client.SendMsg((uint)MSGID.Enterroom, room);
    }

    //房主点击开始游戏，向服务器发送消息
    private void SendStartGame()
    {
        Sync sync = new Sync { Tag = true };
        Client.SendMsg((uint)MSGID.Startgame, sync);
    }

    //玩家切换准备状态，向服务器发送消息
    private void SendReady(Sprite sprite)
    {
        if (sprite == instance.readySprite)
        {
            Sync status = new Sync { Tag = true };
            Client.SendMsg((uint)MSGID.Isready, status);
        }
        else
        {
            Sync status = new Sync { Tag = false };
            Client.SendMsg((uint)MSGID.Isready, status);
        }
    }

    // 点击离开房间时调用，向服务器发送消息
    private void SendExitRoom()
    {
        Sync sync = new Sync { Tag = true };
        Client.SendMsg((uint)MSGID.Leaveroom, sync);

        insideBG.SetActive(false);     //退出房间信息页面

        // 在UI上清空玩家列表
        int playerCount = instance.playerList.Count;
        for (int i = 0; i < playerCount; i++)
            Destroy(instance.playerList[i]);
        // 在数据中清空玩家列表
        instance.playerList.Clear();
        RoomPlayerManager.Instance.ClearRoomPlayer();

        SendRefreshReq();              //离开房间时，刷新一次 
    }

    // 显示创建面板，点击创建房间时调用
    private void ShowCreatePanel()
    {
        createPanel.SetActive(true);
    }

    // 关闭创建面板，点击取消时调用
    private void CloseCreatePanel()
    {
        instance.input.text = "";       //输入框内容置空
        createPanel.SetActive(false);
    }

    // 显示警告面板，MSNetManager也会调用
    public static void ShowWarnPanel(Wrong wrong)
    {
        // 显示面板
        instance.warnPanel.SetActive(true);
        switch (wrong)
        {
            case Wrong.Length:
                instance.wrong = Wrong.Length;
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "房间号长度不能大于6";
                break;
            case Wrong.Digit:
                instance.wrong = Wrong.Digit;
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "房间号只能包含数字";
                break;
            case Wrong.Repeate:
                instance.wrong = Wrong.Repeate;
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "该房间号已存在，请重新输入";
                break;
            case Wrong.EnterFail:
                instance.wrong = Wrong.EnterFail;
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "进入房间失败";
                break;
            case Wrong.NotAllReady:
                instance.wrong = Wrong.NotAllReady;
                instance.warnPanel.transform.GetChild(0).GetComponent<Text>().text = "还有玩家没有准备";
                break;
        }
    }

    // 关闭警告面板
    private void CloseWarnPanel()
    {
        warnPanel.SetActive(false);     // 关闭警告面板
        switch (instance.wrong)
        {
            case Wrong.Length:
                input.text = "";                // 输入框内容置空
                break;
            case Wrong.Digit:
                input.text = "";                // 输入框内容置空
                break;
            case Wrong.Repeate:
                input.text = "";                // 输入框内容置空
                break;
        }
    }

    // 刷新房间列表，MSNetManager调用
    public static void ShowRoomList(RepeatedField<RoomInfo> roomList)
    {
        // 在UI上清空房间列表
        int roomCount = instance.roomList.Count;
        for (int i = 0; i < roomCount; i++)
            Destroy(instance.roomList[i]);
        // 在数据中清空房间列表
        instance.roomList.Clear();
        // 实例化每个房间信息
        foreach (RoomInfo room in roomList)
        {
            // 实例化并设置文本内容
            GameObject roomInfo = Instantiate(instance.roomInfo, instance.content);
            roomInfo.transform.GetChild(0).GetComponent<Text>().text = room.Id.ToString();
            roomInfo.transform.GetChild(1).GetComponent<Text>().text = room.OwnerName;
            roomInfo.transform.GetChild(2).GetComponent<Text>().text = room.Nums.ToString() + "/6";
            // 添加到数据中
            instance.roomList.Add(roomInfo);
            // 添加监听事件
            roomInfo.GetComponent<Button>().onClick.AddListener(delegate { instance.ClickRoom(room.Id); });
        }
    }

    //创建房间，MSNetManager调用
    public static void CreateOneRoom()
    {
        instance.CloseCreatePanel();             //关闭创建面板
        instance.insideBG.SetActive(true);       //进入房间信息页面
        //根据头像序号实例化
        GameObject playerInfo = null;
        switch (User.headIcon)
        {
            case 0:
                playerInfo = Instantiate(instance.playerInfo0, instance.playerParent);
                break;
            case 1:
                playerInfo = Instantiate(instance.playerInfo1, instance.playerParent);
                break;
            case 2:
                playerInfo = Instantiate(instance.playerInfo2, instance.playerParent);
                break;
            case 3:
                playerInfo = Instantiate(instance.playerInfo3, instance.playerParent);
                break;
        }
        playerInfo.transform.GetChild(0).GetComponent<Text>().text = User.name;
        playerInfo.transform.GetChild(1).GetComponent<Text>().text = User.rank.ToString();
        playerInfo.transform.GetChild(2).GetComponent<Image>().sprite = instance.startSprite;
        //给开始游戏图片添加按钮组件
        Button startGame = playerInfo.transform.GetChild(2).gameObject.AddComponent<Button>();
        //添加按钮监听事件
        startGame.onClick.AddListener(instance.SendStartGame);
        //添加到数据中
        instance.playerList.Add(playerInfo);
        instance.roomPlayers.Add(new RoomPlayer(User.uid, User.name, true, true));
        //注意
        RoomPlayerManager.Instance.AddRoomPlayer(User.uid, User.name);
        //设置为房主
        User.isHost = true;
    }

    // 点击房间信息时调用
    public void ClickRoom(int roomID)
    {
        instance.currRoomID = roomID;
    }

    // 进入指定房间，MSNetManager调用
    public static void UpdatePlayerList(RepeatedField<GrpcLibrary.RoomPlayer> playerList, bool isEnter)
    {
        if (isEnter)
        {
            instance.insideBG.SetActive(true);  //进入房间信息页面                                               
            User.isHost = false;                //设置为不是房主
        }
        //在UI上清空玩家列表
        int playerCount = instance.playerList.Count;
        for (int i = 0; i < playerCount; i++)
            Destroy(instance.playerList[i]);
        //在数据中清空玩家列表
        instance.playerList.Clear();
        instance.roomPlayers.Clear();
        //注意
        RoomPlayerManager.Instance.ClearRoomPlayer();
        //实例化每个玩家信息
        foreach (GrpcLibrary.RoomPlayer player in playerList)
        {
            //根据头像序号实例化
            GameObject playerInfo = null;
            switch (player.HeadIcon)
            {
                case 0:
                    playerInfo = Instantiate(instance.playerInfo0, instance.playerParent);
                    break;
                case 1:
                    playerInfo = Instantiate(instance.playerInfo1, instance.playerParent);
                    break;
                case 2:
                    playerInfo = Instantiate(instance.playerInfo2, instance.playerParent);
                    break;
                case 3:
                    playerInfo = Instantiate(instance.playerInfo3, instance.playerParent);
                    break;
            }
            playerInfo.transform.GetChild(0).GetComponent<Text>().text = player.Name;
            playerInfo.transform.GetChild(1).GetComponent<Text>().text = player.Rank.ToString();
            if (player.IsOwner)
            {
                if (User.uid == player.UID)
                {
                    playerInfo.transform.GetChild(2).GetComponent<Image>().sprite = instance.startSprite;
                    Button startGame = playerInfo.transform.GetChild(2).gameObject.AddComponent<Button>();
                    startGame.onClick.AddListener(instance.SendStartGame);
                    User.isHost = true; //设置为房主
                }
                else
                {
                    //其他玩家看房主是已准备状态
                    playerInfo.transform.GetChild(2).GetComponent<Image>().sprite = instance.isReadySprite;
                }
            }
            else
            {
                if (player.IsReady)
                    playerInfo.transform.GetChild(2).GetComponent<Image>().sprite = instance.isReadySprite;
                else
                    playerInfo.transform.GetChild(2).GetComponent<Image>().sprite = instance.readySprite;
                //如果是自己
                if (User.uid == player.UID)
                {
                    Button readyBtn = playerInfo.transform.GetChild(2).gameObject.AddComponent<Button>();
                    readyBtn.onClick.AddListener(delegate { instance.SendReady(playerInfo.transform.GetChild(2).GetComponent<Image>().sprite); });
                }
            }
            //添加到数据中
            instance.playerList.Add(playerInfo);
            instance.roomPlayers.Add(new RoomPlayer(player.UID, player.Name, player.IsReady, player.IsOwner));
            //注意
            RoomPlayerManager.Instance.AddRoomPlayer(player.UID, player.Name);
        }
    }

    // 更新房间内玩家的状态
    public static void UpdatePlayerStatus(string uid, bool isReady)
    {
        for (int i = 0; i < instance.roomPlayers.Count; i++)
        {
            if (instance.roomPlayers[i].uid == uid)
            {
                instance.roomPlayers[i].isReady = isReady;
                instance.playerList[i].transform.GetChild(2).GetComponent<Image>().sprite = (isReady ? instance.isReadySprite : instance.readySprite);
                return;
            }
        }
    }

    // 房主开始了游戏
    public static void StartGame(EnterGame enterGame)
    {
        //为一局内每个玩家设置pid
        RoomPlayerManager.Instance.SetPID(enterGame);
        //设置随机数种子
        User.seed1 = enterGame.Seed1;
        // 在UI上清空玩家列表
        int playerCount = instance.playerList.Count;
        for (int i = 0; i < playerCount; i++)
            Destroy(instance.playerList[i]);
        // 在数据中清空玩家列表
        instance.playerList.Clear();
        // 切换页面
        instance.loadBG.SetActive(true);
        // 调用加载场景的方法
        RSLoadManager.LoadLevel();
    }
}
