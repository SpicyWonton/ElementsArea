package GameApi

import (
	"GameCore"
	"cnet"
	"fmt"
	"github.com/golang/protobuf/proto"
	"inet"
	"log"
	"pbf"
)

//举起方块
type HeaveCubeRouter struct {
	cnet.BaseRouter
}
func(mv *HeaveCubeRouter) Deal(req inet.IReq) {
	log.Println("HeaveCubeRouter")

	//反序列化，获得数据
	msgdata := &pbf.Cube{}


	if err := proto.Unmarshal(req.GetData(), msgdata); err != nil {
		log.Println("CubeRouter Unmarshal err:", err)
		return
	}

	//获得链接属性
	pid, err := req.GetConnection().GetProperty("pid")
	if err != nil {
		fmt.Println("AtkRouter GetProperty err:", err)
		return
	}

	//获得玩家
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

	//获得玩家对应房间
	room := GameCore.MgtRoomObj.GetRoomById(player.RoomID)

	//判断箱子是否已经被举起
	if room.Cube[msgdata.CID] == 0 {
		//将箱子设置为举起的状态
		room.Cube[msgdata.CID] = 1
		data := &pbf.HaveWeapon{
			PID: player.StartId,
			CID: msgdata.CID,
		}
		room.BroadCastALL(uint32(pbf.MSGID_BROHWAVEWEAPON), data)
	}
}

//放下箱子
type PlaceCubeRouter struct {
	cnet.BaseRouter
}
func(mv *PlaceCubeRouter) Deal(req inet.IReq) {
	log.Println("PlaceCubeRouter")

	//反序列化，获得数据
	msgdata := &pbf.Cube{}

	if err := proto.Unmarshal(req.GetData(), msgdata); err != nil {
		log.Println("PlaceCubeRouter Unmarshal err:", err)
		return
	}

	//获得链接属性
	pid, err := req.GetConnection().GetProperty("pid")
	if err != nil {
		fmt.Println("PlaceCubeRouter GetProperty err:", err)
		return
	}

	//获得玩家
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

	//获得玩家对应房间
	room := GameCore.MgtRoomObj.GetRoomById(player.RoomID)
	room.Cube[msgdata.CID] = 0
	data := &pbf.HaveWeapon{
		PID: player.StartId,
		CID: msgdata.CID,
	}
	room.BroadCastALL(uint32(pbf.MSGID_BROPLACEWEAPON), data)
}
