using GrpcLibrary;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomPlayerManager
{
    private static RoomPlayerManager instance;
    public static RoomPlayerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RoomPlayerManager();
                instance.roomPlayers = new List<RoomPlayer>();
            }  
            return instance;
        }
    }
    private RoomPlayerManager() { }

    public class RoomPlayer : IComparable<RoomPlayer>
    {
        public string uid;
        public string name;
        public int pid;
        public int points = 0;
        public int score;   //评分
        public int headIcon;//头像

        
        public RoomPlayer(string uid, string name)
        {
            this.uid = uid;
            this.name = name; 
        }

        public int CompareTo(RoomPlayer other)
        {
            return other.points.CompareTo(points);  //降序
        }
    }

    public List<RoomPlayer> roomPlayers;

    //玩家进入房间时调用
    public void AddRoomPlayer(string uid, string name)
    {
        roomPlayers.Add(new RoomPlayer(uid, name));
    }

    //一局游戏结束时调用
    public void ClearRoomPlayer()
    {
        roomPlayers.Clear();
    }

    //根据uid获得房间内玩家的name
    public string GetName(string uid)
    {
        foreach (var item in roomPlayers)
        {
            if (item.uid == uid)
            {
                return item.name;
            }
        }
        return null;
    }

    //根据pid获得房间内玩家的uid
    public string GetUID(int pid)
    {
        foreach (var item in roomPlayers)
        {
            if (item.pid == pid)
            {
                return item.uid;
            }
        }
        return null;
    }

    //设置每个玩家的pid
    public void SetPID(EnterGame enterGame)
    {
        for (int i = 0; i < enterGame.PlayerList.Count; i++)
        {
            for (int j = 0; j < roomPlayers.Count; j++)
            {
                if (roomPlayers[j].uid == enterGame.PlayerList[i].UID)
                {
                    if (roomPlayers[j].uid == User.uid)
                        User.pid = enterGame.PlayerList[i].PID;
                    roomPlayers[j].pid = enterGame.PlayerList[i].PID;
                    break;
                }
            }
        }
    }

    //为排名场景设置值
    public void SetForRankScene(GameSettleArr data)
    {
        for (int i = 0; i < data.AllData.Count; i++)
        {
            for (int j = 0; j < roomPlayers.Count; j++)
            {
                if (data.AllData[i].UID == roomPlayers[j].uid)
                {
                    if (User.uid == data.AllData[i].UID)
                    {
                        User.rank = data.AllData[i].Rank;
                        User.exp = data.AllData[i].EXP;
                        User.addEXP = data.AllData[i].AddEXP;
                        User.score = data.AllData[i].Score;
                    }
                    roomPlayers[j].score = data.AllData[i].Score;
                    roomPlayers[j].headIcon = data.AllData[i].HeadIcon;
                    break;
                }
            }
        }
    }
}
