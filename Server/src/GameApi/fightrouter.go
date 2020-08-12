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

/*
	游戏中的玩家行为的处理和转发
	移动（速度）、攻击、举起武器、血量变化
 */

//移动、速度
type MoveRouter struct {
	cnet.BaseRouter
}
func(mv *MoveRouter) Deal(req inet.IReq) {
	//log.Println("MoveRouter")

	//反序列化，获得数据
	msgdata := &pbf.Player{}
	if err := proto.Unmarshal(req.GetData(), msgdata);err != nil {
		fmt.Println("MoveRouter Unmarshal err:", err)
		return
	}

	//得到该消息是从那个玩家传递来的
	pid, err := req.GetConnection().GetProperty("pid")
	if err != nil {
		fmt.Println("GetProperty err:", err)
		return
	}

	//获得玩家实例
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))
	player.Move(msgdata.Pos, msgdata.Rot, msgdata.Speed, msgdata.HP)
}


//攻击
type AtkRouter struct {
	cnet.BaseRouter
}
func(mv *AtkRouter) Deal(req inet.IReq) {
	log.Println("AtkRouter")

	//反序列化，获得数据
	msgdata := &pbf.Vector{}
	if err := proto.Unmarshal(req.GetData(), msgdata); err != nil {
		log.Println("MoveRouter Unmarshal err:", err)
		return
	}

	//获得链接属性
	pid, err := req.GetConnection().GetProperty("pid")
	if err != nil {
		fmt.Println("AtkRouter GetProperty err:", err)
		return
	}

	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))
	player.Atk(msgdata.X,msgdata.Y,msgdata.Z)
}

//方块碰撞
type CollisionRouter struct {
	cnet.BaseRouter
}
func(mv *CollisionRouter) Deal(req inet.IReq) {
	log.Println("CollisionRouter")
	//
	//反序列化，获得数据
	msgdata := &pbf.CubeColl{}
	if err := proto.Unmarshal(req.GetData(), msgdata); err != nil {
		log.Println("MoveRouter Unmarshal err:", err)
		return
	}

	//获得链接属性
	pid, err := req.GetConnection().GetProperty("pid")
	if err != nil {
		fmt.Println("AtkRouter GetProperty err:", err)
		return
	}

	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))
	room := GameCore.MgtRoomObj.GetRoomById(player.RoomID)
	room.BroadCast(128, msgdata, player)
}