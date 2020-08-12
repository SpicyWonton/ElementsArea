package GameApi

import (
	"GameCore"
	"db"
	"inet"
	"log"
	"pbf"
)

/*
	绑定游戏中的所有路由
	设置了玩家下线断开链接前的处理（从在线列表中移除玩家、退出房间、移除链接属性等）
 */


//玩家断开链接前的处理函数
func DoLost(conn inet.IConnection) {
	log.Printf("[DoLost bindrouter]玩家下线/断开连接")

	//通过连接属性获取绑定的玩家pid
	pid,err := conn.GetProperty("pid")
	if err != nil {	//获取链接属性失败--登录失败并未设置链接属性，断开了链接
		log.Println("GetProperty error err:", err)
		return
	}

	//移除该链接的属性
	conn.RemoveProperty("pid")
	//获取玩家对象
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

	defer func() {
		err := recover()
		if err != nil {
			log.Println("[DoLost bindrouter]",err)
		}
	}()

	//判断玩家是否在房间中
	if player.RoomID != 0 { //玩家在房间中
		GameCore.MgtRoomObj.LeaveRoom(player) //将玩家从房间中移除
		//执行玩家下线的广播通知
		room := GameCore.MgtRoomObj.GetRoomById(player.RoomID)

		//判断该玩家离开房间后，房间是否还存在，存在即广播该玩家离开房间的消息
		if room != nil {
			data := &pbf.EnterRoom{}
			players := room.GetAllPlayer()
			for _, v := range players {
				tempplayer := &pbf.RoomPlayer{}
				tempplayer.UID = v.Id
				tempplayer.IsOwner = v.IsOwner
				tempplayer.IsReady = v.Status
				tempplayer.Rank = v.Pmsg.Rank
				tempplayer.HeadIcon = v.Pmsg.HeadIcon
				tempplayer.Name = v.Pmsg.Name
				data.PlayerList = append(data.PlayerList, tempplayer)
			}
			room.BroadCast(uint32(pbf.MSGID_BROROOMCHANGE), data,player)
		}
	}
	//将玩家的等级和经验持久化到数据库中
	db.DbObj.UpdatePlayer(player.Id, player.Pmsg.Experience, player.Pmsg.Rank)

	//从在线玩家列表中把玩家删除
	GameCore.MgtplayerObj.DeletePlayer(pid.(string))
}


func AddAllRouter(s inet.IServer) {
	//注册、登录的api

	//账号相关
	s.AddRouter(uint32(pbf.MSGID_SIGNUP),&registeredRouter{})
	s.AddRouter(uint32(pbf.MSGID_SIGNIN),&LoginRouter{})

	////房间相关
	s.AddRouter(uint32(pbf.MSGID_ROOMINFO), &GetRoomList{})
	s.AddRouter(uint32(pbf.MSGID_CREATEROOM),&CreateRoom{})
	s.AddRouter(uint32(pbf.MSGID_ENTERROOM), &EnterRoom{})
	s.AddRouter(uint32(pbf.MSGID_LEAVEROOM), &LeaveRoom{})
	s.AddRouter(uint32(pbf.MSGID_ISREADY), &ChangeStatus{})

	//游戏局内相关
	s.AddRouter(uint32(pbf.MSGID_STARTGAME), &StartGame{})
	s.AddRouter(uint32(pbf.MSGID_LOADMAP), &LoadFinishedRouter{})
	s.AddRouter(uint32(pbf.MSGID_SYNCPHV), &MoveRouter{})
	s.AddRouter(uint32(pbf.MSGID_HWAVEWEAPON), &HeaveCubeRouter{})
	s.AddRouter(uint32(pbf.MSGID_PLACECUBE), &PlaceCubeRouter{})
	s.AddRouter(uint32(pbf.MSGID_ATK), &AtkRouter{})
	s.AddRouter(uint32(pbf.MSGID_COLLISION), &CollisionRouter{})
	s.AddRouter(uint32(pbf.MSGID_ADDSCORE), &AddScoreRouter{})
	s.AddRouter(uint32(pbf.MSGID_VOICE), &TalkVoiceRouter{})

	//绑定玩家下线前执行的函数
	s.SetLostConn(DoLost)
}