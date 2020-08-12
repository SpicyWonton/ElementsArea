using Google.Protobuf;
using GrpcLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public delegate void CallBack(Message msg);

public static class Client
{
    private class NetManager : MonoBehaviour
    {
        private static NetManager instance;         //单例模式                                          
        private Dictionary<uint, CallBack> router = new Dictionary<uint, CallBack>();    //根据消息ID选择对应的回调函数
        private Queue<Message> msgs = new Queue<Message>();
        private event Action ApplicationQuitEvent;  //程序退出时回调事件

        public static NetManager Instance
        {
            get
            {
                if (!instance)
                {
                    GameObject CientObj = new GameObject("NetManager");
                    instance = CientObj.AddComponent<NetManager>();
                    DontDestroyOnLoad(CientObj);
                    RegisterAll();  //第一次实例化时，注册所有事件
                }
                return instance;
            }
        }

        private void Update()
        {
            int count = msgs.Count;
            while (count > 0)
            {
                Message msg = msgs.Dequeue();
                try
                {
                    router[msg.id](msg);
                }
                catch (Exception e)
                {
                    Debug.Log("消息ID：" + msg.id + "===" + e.Message);
                }
                count--;
            }
        }

        //注册指定消息回调事件
        private static void Register(uint msgID, CallBack func)
        {
            if (!instance.router.ContainsKey(msgID))
                instance.router.Add(msgID, func);
            else
                Debug.LogWarning("已经注册了该事件，不要重复注册");
        }

        //注册所有消息回调事件
        private static void RegisterAll()
        {
            //游戏外的事件
            Register((uint)MSGID.Signup, CallBackSet.SignUp);                   //注册是否成功
            Register((uint)MSGID.Signin, CallBackSet.SignIn);                   //登录是否成功
            Register((uint)MSGID.Roominfo, CallBackSet.UpdateRoomList);         //刷新房间列表
            Register((uint)MSGID.Createroom, CallBackSet.CreateRoom);           //创建房间是否成功
            Register((uint)MSGID.Enterroom, CallBackSet.EnterInToRoom);         //进入房间是否成功
            Register((uint)MSGID.Broroomchange, CallBackSet.UpdatePlayerList);  //刷新房间内的玩家列表
            Register((uint)MSGID.Broisready, CallBackSet.UpdatePlayerStatus);   //刷新房间内的玩家状态
            Register((uint)MSGID.Startgame, CallBackSet.NotAllReady);           //开始游戏失败
            Register((uint)MSGID.Setpid, CallBackSet.StartGame);                //开始游戏成功
            Register((uint)MSGID.Broentergame, CallBackSet.EnterInToGame);      //所有玩家都加载完成
            //游戏内的事件
            Register((uint)MSGID.Brosyncphv, CallBackSet.UpdateTransform);      //更新玩家信息（位置、旋转、速度）
            Register((uint)MSGID.Brovoice, CallBackSet.UpdateVoice);            //同步语音
            Register((uint)MSGID.Brohwaveweapon, CallBackSet.UpdatePick);       //更新玩家举起状态
            Register((uint)MSGID.Broplaceweapon, CallBackSet.UpdatePlace);      //更新玩家放下状态
            Register((uint)MSGID.Broatk, CallBackSet.UpdateAttack);             //同步玩家的攻击
            Register((uint)MSGID.Brocollision, CallBackSet.UpdateCubeColl);     //更新方块碰撞
            Register((uint)MSGID.Broaddscore, CallBackSet.UpdatePoints);        //更新游戏分数
            Register((uint)MSGID.Gamefin, CallBackSet.GameOver);                //游戏结束
        }

        //入队
        public void Enqueue(Message msg)
        {
            instance.msgs.Enqueue(msg);
        }

        //设置退出事件
        public void SetQuitEvent(Action action)
        {
            if (ApplicationQuitEvent != null)
                return;
            ApplicationQuitEvent += action;
        }

        //程序退出时自动调用
        private void OnApplicationQuit()
        {
            if (ApplicationQuitEvent != null)
                ApplicationQuitEvent();
        }
    }

    private static IPAddress ip;    //服务器ip地址
    private static int port;        //服务器端口号         

    //客户端状态
    private enum ClientState
    {
        None,   //未连接
        Conn    //已连接
    }

    private static ClientState currState;   //当前状态
    private static TcpClient client;
    private static NetworkStream stream;

    private static IEnumerator Receive()
    {
        while (currState == ClientState.Conn)
        {
            byte[] headData = new byte[8];  //消息头部固定8字节
            byte[] data;                    //消息数据
            int recvLen;                    //接收到的长度

            IAsyncResult ar = stream.BeginRead(headData, 0, headData.Length, null, null);
            while (!ar.IsCompleted)
            {
                yield return null;
            }
            //异常处理
            try
            {
                recvLen = stream.EndRead(ar);
            }
            catch (Exception e)
            {
                currState = ClientState.None;
                Debug.LogError("消息头接收失败" + e.Message);
                yield break;
            }
            if (recvLen < headData.Length)
            {
                currState = ClientState.None;
                Debug.LogError("消息头接收失败");
                yield break;
            }
            //拆包
            Message msg = DataPack.UnPack(headData);
            //读取数据内容
            if (msg.dataLen > 0)
            {
                data = new byte[msg.dataLen];
                ar = stream.BeginRead(data, 0, data.Length, null, null);
                while (!ar.IsCompleted)
                {
                    yield return null;
                }
                try
                {
                    recvLen = stream.EndRead(ar);
                }
                catch (Exception e)
                {
                    currState = ClientState.None;
                    Debug.LogError("消息内容接收失败" + e.Message);
                    yield break;
                }
                if (recvLen < data.Length)
                {
                    currState = ClientState.None;
                    Debug.LogError("消息内容接收失败");
                    yield break;
                }
                msg.data = data;
            }
            else
            {
                data = new byte[0];
                msg.data = data;
            }
            Log.AddContent("收到消息ID：" + msg.id + "时间戳：" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds());
            //处理业务
            NetManager.Instance.Enqueue(msg);
        }
    }

    //点击登录时调用，与服务器建立连接
    public static void Connect(string addr, int port_)
    {
        //不能重复连接
        if (currState == ClientState.Conn)
        {
            Debug.LogWarning("已经连接上服务器，不要重复连接");
            return;
        }
        //类型转换
        if (!IPAddress.TryParse(addr, out ip))
        {
            Debug.LogError("IP地址错误");
            return;
        }
        port = port_;
        //建立连接
        client = new TcpClient();
        try
        {
            client.Connect(ip, port);
        }
        catch
        {
            Debug.LogError("建立连接失败");
            return;
        }
        //连接成功
        Debug.Log("连接服务器成功");
        stream = client.GetStream();
        currState = ClientState.Conn;
        //开启读协程
        NetManager.Instance.StartCoroutine(Receive());
        NetManager.Instance.SetQuitEvent(() => { client.Close(); currState = ClientState.None; });
    }

    //用户发消息时调用
    public static void SendMsg(uint msgID, IMessage protoMsg)
    {
        try
        {
            // 将数据序列化为字节数组
            byte[] data = protoMsg.ToByteArray();
            // 定义一个消息类
            Message msg = new Message(msgID, (uint)data.Length, data);
            // 将消息类打包成字节数组
            byte[] binaryMsg = DataPack.Pack(msg);
            if (currState == ClientState.Conn)
                stream.Write(binaryMsg, 0, binaryMsg.Length);
        }
        catch (Exception e)
        {
            currState = ClientState.None;
            Debug.LogError("发送消息失败" + e.Message);
            return;
        }
    }

    //与服务器断开连接
    public static void CloseConn()
    {
        client.Close();
        currState = ClientState.None;
    }
}
