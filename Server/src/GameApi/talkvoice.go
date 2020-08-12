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

//放下箱子
type TalkVoiceRouter struct {
	cnet.BaseRouter
}
func(mv *TalkVoiceRouter) Deal(req inet.IReq) {
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

	room.BroadCast(uint32(pbf.MSGID_BROVOICE), msgdata,player)
}
