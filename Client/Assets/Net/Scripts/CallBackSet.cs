using GrpcLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CallBackSet
{
    //注册是否成功
    public static void SignUp(Message msg)
    {
        SignSucc signSucc = SignSucc.Parser.ParseFrom(msg.data);
        if (signSucc.Flag)
        {
            //设置用户信息
            User.uid = signSucc.UserInfo.UID;
            User.password = signSucc.UserInfo.Password;
            User.name = signSucc.UserInfo.Name;
            User.rank = signSucc.UserInfo.Rank;
            User.exp = signSucc.UserInfo.EXP;
            User.headIcon = signSucc.UserInfo.HeadIcon;
            //进入下一个场景
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            LoginManager.ShowWarnPanel(LoginManager.Wrong.SignUpFaliure);
            Client.CloseConn();
        }
    }

    //登录是否成功
    public static void SignIn(Message msg)
    {
        SignSucc signSucc = SignSucc.Parser.ParseFrom(msg.data);
        if (signSucc.Flag)
        {
            //设置用户信息
            User.uid = signSucc.UserInfo.UID;
            User.password = signSucc.UserInfo.Password;
            User.name = signSucc.UserInfo.Name;
            User.rank = signSucc.UserInfo.Rank;
            User.exp = signSucc.UserInfo.EXP;
            User.headIcon = signSucc.UserInfo.HeadIcon;
            //进入下一个场景
            SceneManager.LoadScene("MainScene");
        }
        else
        {
            LoginManager.ShowWarnPanel(LoginManager.Wrong.SignInFaliure);
            Client.CloseConn();
        }
    }

    // 接收服务器对于创建房间的反馈消息
    public static void CreateRoom(Message msg)
    {
        Sync sync = Sync.Parser.ParseFrom(msg.data);
        // Tag为True表示创建房间成功，反之
        if (sync.Tag)
            RSUIManager.CreateOneRoom();
        else
            RSUIManager.ShowWarnPanel(RSUIManager.Wrong.Repeate);
    }

    // 接收服务器的房间列表
    public static void UpdateRoomList(Message msg)
    {
        RoomList roomList = RoomList.Parser.ParseFrom(msg.data);

        foreach (RoomInfo room in roomList.Roomlist)
        {
            Debug.Log("房间ID：" + room.Id + "房间人数：" + room.Nums + "房主昵称：" + room.OwnerName);
        }

        RSUIManager.ShowRoomList(roomList.Roomlist);
    }

    // 接收服务器对于进入房间的反馈信息
    public static void EnterInToRoom(Message msg)
    {
        EnterRoom roomInfo = EnterRoom.Parser.ParseFrom(msg.data);

        foreach (RoomPlayer player in roomInfo.PlayerList)
        {
            Debug.Log("玩家ID：" + player.Name + "是否是房主：" + player.IsOwner + "是否准备" + player.IsReady);
        }

        // Flag为True表示进入房间成功，反之
        if (roomInfo.Flag)
            RSUIManager.UpdatePlayerList(roomInfo.PlayerList, true);
        else
            RSUIManager.ShowWarnPanel(RSUIManager.Wrong.EnterFail);
    }

    // 更新房间内的玩家信息，有其他玩家进入该房间时会调用
    public static void UpdatePlayerList(Message msg)
    {
        EnterRoom roomInfo = EnterRoom.Parser.ParseFrom(msg.data);

        foreach (RoomPlayer player in roomInfo.PlayerList)
        {
            Debug.Log("玩家ID：" + player.Name + "是否是房主：" + player.IsOwner + "是否准备" + player.IsReady);
        }

        RSUIManager.UpdatePlayerList(roomInfo.PlayerList, false);
    }

    // 更新房间内玩家的准备状态，其他玩家切换状态时会调用
    public static void UpdatePlayerStatus(Message msg)
    {
        IsReady status = IsReady.Parser.ParseFrom(msg.data);

        Debug.Log("切换准备状态的玩家ID：" + status.UID + "当前准备状态：" + status.Ready);

        RSUIManager.UpdatePlayerStatus(status.UID, status.Ready);
    }

    // 当有人没准备的时候房主点击了开始游戏，房主会收到服务的消息
    public static void NotAllReady(Message msg)
    {
        RSUIManager.ShowWarnPanel(RSUIManager.Wrong.NotAllReady);
    }

    // 成功开始游戏，给每个人分配游戏内的id
    public static void StartGame(Message msg)
    {
        EnterGame enterGame = EnterGame.Parser.ParseFrom(msg.data);
        RSUIManager.StartGame(enterGame);
    }

    // 所有玩家都加载完成，可以进入游戏了
    public static void EnterInToGame(Message msg)
    {
        GameTime gameTime = GameTime.Parser.ParseFrom(msg.data);
        User.gameTime = gameTime.TimeLeft;
        RSLoadManager.SetCanEnter();
    }

    /* 游戏内 */

    // 更新其他玩家移动属性
    public static void UpdateTransform(Message msg)
    {
        //Log.LogFile("处理完消息ID：" + msg.id + "时间戳：" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds());
        BroadCastMove bc = BroadCastMove.Parser.ParseFrom(msg.data);
        // 通过Pid找到对应的玩家
        if (GameManager.instance != null)
        {
            for (int i = 0; i < bc.PlayerInfo.Count; i++)
            {
                GameObject targetPlayer = GameManager.instance.players[bc.PlayerInfo[i].PID];
                if (bc.PlayerInfo[i].PID != User.pid)
                {
                    targetPlayer.GetComponent<PlayerControl>().canLerp = true;
                    targetPlayer.GetComponent<PlayerControl>().targetPos = new Vector3(bc.PlayerInfo[i].Pos.X, bc.PlayerInfo[i].Pos.Y, bc.PlayerInfo[i].Pos.Z);
                    targetPlayer.GetComponent<PlayerControl>().model.transform.eulerAngles = new Vector3(bc.PlayerInfo[i].Rot.X, bc.PlayerInfo[i].Rot.Y, bc.PlayerInfo[i].Rot.Z);
                    targetPlayer.GetComponent<PlayerManager>().currentHp = bc.PlayerInfo[i].HP;
                    targetPlayer.GetComponent<PlayerManager>().AddHp(0);   //更新血量和血条
                    //更新玩家速度
                    targetPlayer.GetComponent<PlayerControl>().targetSpeed = bc.PlayerInfo[i].Speed;
                }
            }
        }
        GameManager.instance.canSend = true;
    }

    //同步举方块
    public static void UpdatePick(Message msg)
    {
        HaveWeapon hw = HaveWeapon.Parser.ParseFrom(msg.data);
        GameManager.instance.players[hw.PID].GetComponent<PlayerControl>().Pick(hw.CID);
    }

    //同步放方块
    public static void UpdatePlace(Message msg)
    {
        HaveWeapon hw = HaveWeapon.Parser.ParseFrom(msg.data);
        GameManager.instance.players[hw.PID].GetComponent<PlayerControl>().Place();
    }

    //同步玩家的攻击
    public static void UpdateAttack(Message msg)
    {
        PlayerAtk pa = PlayerAtk.Parser.ParseFrom(msg.data);
        // 获取投掷方向
        Vector3 attackDir = new Vector3(pa.AttackDir.X, pa.AttackDir.Y, pa.AttackDir.Z);
        // 调用对应玩家的攻击函数
        GameManager.instance.players[pa.PID].GetComponent<PlayerControl>().AttackAction(attackDir);
    }

    //同步方块碰撞
    public static void UpdateCubeColl(Message msg)
    {
        CubeColl cubeColl = CubeColl.Parser.ParseFrom(msg.data);
        CubeManager.UpdateCollision(cubeColl);
    }

    //更新游戏内分数
    public static void UpdatePoints(Message msg)
    {
        PlayerPoint playerPoint = PlayerPoint.Parser.ParseFrom(msg.data);
        GameManager.UpdatePoints(playerPoint.UID, playerPoint.Point);
    }

    //游戏结束
    public static void GameOver(Message msg)
    {
        GameSettleArr data = GameSettleArr.Parser.ParseFrom(msg.data);
        RoomPlayerManager.Instance.SetForRankScene(data);
        SceneManager.LoadScene("RankScene");
    }

    public static void UpdateVoice(Message msg)
    {
        TalkVoice talkVoice = TalkVoice.Parser.ParseFrom(msg.data);
        AudioSource voice =  GameManager.instance.players[talkVoice.PID].GetComponent<PlayerManager>().voicePos.GetComponent<AudioSource>();
        AudioClip[] audioclip = GameObject.Find("Audio Button").GetComponent<VoiceAudio>().audioClips;
        GameManager.instance.players[talkVoice.PID].GetComponent<PlayerManager>().SetAudioImg(true,audioclip[talkVoice.VID].length);
        voice.clip = audioclip[talkVoice.VID];
        voice.Play();
    }
}
