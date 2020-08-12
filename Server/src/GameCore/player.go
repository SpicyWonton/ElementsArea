package GameCore

import (
	"inet"
	"log"
	"pbf"
	"sync"

	"github.com/golang/protobuf/proto"
)

type Player struct {
	Id      string           //玩家ID
	Conn    inet.IConnection //当前玩家的连接
	P       *pbf.Vector      //玩家的位置
	R       *pbf.Vector      //玩家的旋转位置
	Hp      float32          //玩家的血量
	Speed   float32          //玩家的速度
	Status  bool             //未准备， 准备
	RoomID  int32            //房间id，0代表不在房间中
	StartId int32            //在房间内的各玩家id
	IsOwner bool             //是否是房主
	Pmsg    PlayerMsg        //玩家持久化数据
	Rwm     sync.Mutex
}

//广播玩家的位置的消息
func (player *Player) Move(position *pbf.Vector, rotation *pbf.Vector, speed float32, hp float32) {
	//位置相同，不转发
	//if player.P != nil && player.P.X == position.X && player.P.Y == position.Y && player.P.Z == position.Z {
	//	return
	//}

	player.P = position
	player.R = rotation
	player.Speed = speed
	player.Hp = hp
	//fmt.Println(player.RoomID)
	//if player.RoomID == 0 {
	//	fmt.Println("xxxxx")
	//}

	//room := MgtRoomObj.GetRoomById(player.RoomID)
	//
	//
	////player.Rwm.Lock()
	////room.IsRecv[player.StartId] = true
	////player.Rwm.Unlock()
	//if room == nil {
	//	return
	//}
	//room.IStest = room.IStest + 1
	//
	//fmt.Println("收到了帧 ",player.StartId)
	////说明已经收到全部的帧
	////fmt.Println(len(room.IsRecv),len(room.RoomPlayers))
	////len(room.IsRecv) == len(room.RoomPlayers)
	//if room.IStest == len(room.RoomPlayers) {
	//	fmt.Println("广播帧")
	//	//获得所有的玩家
	//	players := room.GetAllPlayer()
	//
	//	msgdata := &pbf.BroadCastMove{
	//		PlayerInfo: nil,
	//	}
	//
	//	for _, v := range players {
	//		infolist := &pbf.Player{
	//			Pos:   v.P,
	//			Rot:   v.R,
	//			Speed: v.Speed,
	//			Pid:   v.StartId,
	//			HP:	   v.Hp,
	//		}
	//		msgdata.PlayerInfo = append(msgdata.PlayerInfo, infolist)
	//	}
	//	room.BroadCastALL(uint32(pbf.MSGID_BROSYNCPHV), msgdata)
	//	//清空map数组，这一帧结束
	//	//player.Rwm.Lock()
	//	//room.IsRecv = make(map[int32]bool)
	//	//player.Rwm.Unlock()
	//	room.IStest = 0
	//}
}

//广播玩家攻击
func (player *Player) Atk(x, y, z float32) {

	data := &pbf.PlayerAtk{
		Pid: player.StartId,
		AttackDir: &pbf.Vector{
			X: x,
			Y: y,
			Z: z,
		},
	}

	player.PlayerBroadCast(uint32(pbf.MSGID_BROATK), data)
}

//广播玩家状态的封装
func (player *Player) PlayerBroadCast(msgId uint32, data proto.Message) {
	//获取玩家所在的房间
	room := MgtRoomObj.GetRoomById(player.RoomID)
	//让房间广播信息
	room.BroadCast(msgId, data, player)
}

//发送消息给客户端， 将数据序列化之后发送
func (player *Player) SendMsg(MsgId uint32, data proto.Message) {
	msg, _ := proto.Marshal(data)

	//调用框架的 SendMsg 方法
	if err := player.Conn.SendMsg(MsgId, msg); err != nil {
		log.Println("Player SendMsg error !")
		return
	}
}
