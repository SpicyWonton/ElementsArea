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

//处理玩家得分变化
type AddScoreRouter struct {
	cnet.BaseRouter
}
func(mv *AddScoreRouter) Deal(req inet.IReq) {
	log.Println("AddScoreRouter")

	msgdata := &pbf.PlayerPoint{}
	if err := proto.Unmarshal(req.GetData(), msgdata); err != nil {
		log.Println("MoveRouter Unmarshal err:", err)
		return
	}

	//得到该消息是从那个玩家传递来的
	pid, err := req.GetConnection().GetProperty("pid")
	if err != nil {
		fmt.Println("AddScore GetProperty err:", err)
		return ;
	}

	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))
	room := GameCore.MgtRoomObj.GetRoomById(player.RoomID)


	//判断不同的得分
	if room.Score == nil {
		fmt.Println("room score不存在")
		return
	}
	room.Score[player.StartId] += int(msgdata.Point)

	data := &pbf.PlayerPoint{
		UID:   player.Id,
		Point: int32(room.Score[player.StartId]),
	}
	room.BroadCastALL(uint32(pbf.MSGID_BROADDSCORE),data)
}
