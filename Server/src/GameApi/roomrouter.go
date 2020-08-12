package GameApi

import (
	"GameCore"
	"cnet"
	"fmt"
	"github.com/golang/protobuf/proto"
	"inet"
	"log"
	"math/rand"
	"pbf"
	"time"
)

//创建房间
type CreateRoom struct {
	cnet.BaseRouter
}
func(mv *CreateRoom) Deal(req inet.IReq) {
	log.Println("[CreateRoom Api]")

	//将数据反序列化
	msgdata := &pbf.Room{}
	err := proto.Unmarshal(req.GetData(), msgdata)
	if err != nil {
		log.Println("MoveRouter Unmarshal err:", err)
	}

	//获取链接属性和具体玩家
	pid, _ := req.GetConnection().GetProperty("pid")
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

	//创建房间
	if GameCore.MgtRoomObj.CreateRoom(msgdata.Rid, 6, player) == true{
		//成功
		player.SendMsg(uint32(pbf.MSGID_CREATEROOM),&pbf.Sync{Tag: true,})
	} else {
		//失败
		player.SendMsg(uint32(pbf.MSGID_CREATEROOM),&pbf.Sync{Tag: false,})
	}
}

//获取房间列表
type GetRoomList struct {
	cnet.BaseRouter
}
func(mv *GetRoomList) Deal(req inet.IReq) {
	log.Println("[GetRoomList Api]")

	msgdata := &pbf.Sync{}
	err := proto.Unmarshal(req.GetData(), msgdata)
	if err != nil {
		log.Println("GetRoomList Unmarshal err:", err)
	}

	//获取链接属性和具体玩家
	pid, _ := req.GetConnection().GetProperty("pid")
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

	//将房间的信息导出
	roominfoarr := GameCore.MgtRoomObj.GetRoomList()
	data := &pbf.RoomList{}
	for _, v := range roominfoarr {
		tempinfo := &pbf.RoomInfo{
			Id:      v.Id,
			Nums:    v.Nums,
			OwnerName: v.OwnerName,
		}
		data.Roomlist = append(data.Roomlist, tempinfo)
	}
	player.SendMsg(uint32(pbf.MSGID_ROOMINFO),data)
}

//进入房间
type EnterRoom struct {
	cnet.BaseRouter
}
func(mv *EnterRoom) Deal(req inet.IReq) {
	log.Println("[EnterRoom Api]")

	msgdata := &pbf.Room{}
	err := proto.Unmarshal(req.GetData(), msgdata)
	if err != nil {
		log.Println("MoveRouter Unmarshal err:", err)
	}

	pid, _ := req.GetConnection().GetProperty("pid")
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

	//找到要加入的房间
	room := GameCore.MgtRoomObj.GetRoomById(msgdata.Rid)
	if room == nil {
		log.Println("该房间不存在，无法加入")
		return
	}
	//将请求的玩家加入该房间
	room.AddPlayer(player)

	data := &pbf.EnterRoom{
		Flag:       true,
		PlayerList: nil,
	}

	//如果能进入房间，将房间内的玩家信息进行广播
	if data.Flag == true {
		players := room.GetAllPlayer()
		for _, v := range players {
			tempplayer := &pbf.RoomPlayer{}
			tempplayer.UID = v.Id
			tempplayer.IsOwner = v.IsOwner
			tempplayer.IsReady = v.Status
			tempplayer.Name = v.Pmsg.Name
			tempplayer.Rank = v.Pmsg.Rank
			tempplayer.HeadIcon = v.Pmsg.HeadIcon
			data.PlayerList = append(data.PlayerList, tempplayer)
		}
		player.SendMsg(uint32(pbf.MSGID_ENTERROOM),data)
		room.BroadCast(uint32(pbf.MSGID_BROROOMCHANGE), data,player)
	} else {
		player.SendMsg(uint32(pbf.MSGID_ENTERROOM),data)
	}


}


//离开房间
type LeaveRoom struct {
	cnet.BaseRouter
}
func(mv *LeaveRoom) Deal(req inet.IReq) {
	log.Println("[LeaveRoom api-room]")
	pid, _ := req.GetConnection().GetProperty("pid")
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

	GameCore.MgtRoomObj.LeaveRoom(player)
	//查询该房间是否还存在
	room := GameCore.MgtRoomObj.GetRoomById(player.RoomID)
	player.RoomID = 0 //将该玩家的房间id设置为0，代表不在房间内
	player.P = nil
	player.Status = false

	if room != nil { //该房间还存在--则要广播该玩家离开房间的消息
		data := &pbf.EnterRoom{}

		//获取房间中剩余的玩家信息
		players := room.GetAllPlayer()
		//遍历剩余玩家信息，添加到数组中
		for _, v := range players {
			tempplayer := &pbf.RoomPlayer{}
			tempplayer.UID = v.Id
			tempplayer.IsOwner = v.IsOwner
			tempplayer.IsReady = v.Status
			tempplayer.Rank = v.Pmsg.Rank
			tempplayer.HeadIcon = v.Pmsg.HeadIcon
			tempplayer.Name = v.Pmsg.Name
			data.PlayerList = append(data.PlayerList, tempplayer)
			fmt.Println("name",v.Pmsg.Name)
		}
		//广播给房间中剩余的玩家
		room.BroadCastALL(uint32(pbf.MSGID_BROROOMCHANGE), data)
	}
}

//玩家准备/取消准备
type ChangeStatus struct {
	cnet.BaseRouter
}
func(mv *ChangeStatus) Deal(req inet.IReq) {
	log.Println("[ChangeStatus api-room]")

	pid, _ := req.GetConnection().GetProperty("pid")
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))


	msgdata := &pbf.Sync{}
	err := proto.Unmarshal(req.GetData(), msgdata)
	if err != nil {
		log.Println("ChangeStatus Unmarshal err:", err)
	}

	//设置玩家的准备状态
	player.Status = msgdata.Tag

	//获取房间
	room := GameCore.MgtRoomObj.GetRoomById(player.RoomID)
	data := &pbf.IsReady{
		UID:   pid.(string),
		Ready: msgdata.Tag,
	}

	//在房间内广播玩家的准备状态
	room.BroadCastALL(uint32(pbf.MSGID_BROISREADY),data)
}

//房主请求开始游戏
type StartGame struct {
	cnet.BaseRouter
}
func(sg *StartGame) Deal(req inet.IReq) {
	log.Println("[StartGame api-room]")

	pid, _ := req.GetConnection().GetProperty("pid")
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

	//获取当前房间
	room := GameCore.MgtRoomObj.GetRoomById(player.RoomID)

	//判断是否每个玩家都准备了
	flag := true
	for _, v := range room.RoomPlayers {
		if v.Status == false {
			flag = false
			break
		}
	}

	if flag == true {
		//如果玩家均为准备状态，则广播发送可以开始游戏

		sumplayers := len(room.RoomPlayers)
		rand.Seed(time.Now().Unix())
		arr := [10]int32{1,2}
		m := map[int32]bool{}
		i := 0
		for {
			x := int32(rand.Intn(sumplayers))
			if _,ok := m[x]; !ok {
				arr[i] = x
				i = i + 1
				m[x] = true
			}
			if (i == sumplayers) {
				break
			}
		}

		//给每个玩家进行编号
		i = 0
		msgdata := &pbf.EnterGame{
			PlayerList: nil,
			Seed1:      int32(rand.Intn(10000)),
		}
		for _, v := range room.RoomPlayers {
			data := &pbf.GamePlayer{
				UID: v.Id,
				PID: arr[i],
			}
			msgdata.PlayerList = append(msgdata.PlayerList, data)
			v.StartId = arr[i]
			i++
			//每个玩家的游戏局数增加1
			v.Pmsg.GameTimes++
		}
		room.BroadCastALL(uint32(pbf.MSGID_SETPID),msgdata)
		//将房间状态设置为游戏中
		room.Status = true

	} else {
		//如果有玩家没有准备，则给房主发消息
		player.SendMsg(uint32(pbf.MSGID_STARTGAME),&pbf.Sync{Tag: false,})
	}
}

