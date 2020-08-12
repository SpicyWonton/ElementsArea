package GameCore

import (
	"github.com/golang/protobuf/proto"
	"log"
	"sync"
)

type Room struct {
	Rid int32						//房间id
	Status bool             		//房间的两种状态：准备中、游戏中
	maxPlayer int32         		//一个房间内最大玩家数量
	RoomPlayers map[string] *Player //房间中的玩家列表
	LoadFin int32					//加载完地图的玩家个数
	StartTime int32					//游戏的开始时间
	Score []int						//玩家得分
	RoomLock sync.RWMutex			//房间的锁
	//IsRecv map[int32]bool			//是否已经收到消息
	IStest int						//收到几个玩家的帧
	Cube []int						//方块的状态
	GAMEEXIT chan bool					//游戏结束
}

//根据房间id和最大人数限制创建一个房间
func NewRoom(rid int32, maxplayer int32) *Room{
	room := Room{
		Rid:	   rid,
		Status:    false,
		maxPlayer: maxplayer,
		RoomPlayers:      make(map[string]*Player,maxplayer),
		LoadFin : 0,
		Score: make([]int,6,6),
		//IsRecv: make(map[int32]bool,6),
		Cube: make([]int, 1000, 1000),
		GAMEEXIT : make(chan bool, 1),
	}
	return &room
}


//进入房间-添加一个玩家
func (r *Room) AddPlayer(player *Player) bool{
	log.Println("[AddPlayer Room]")

	sumPlayers := len(r.RoomPlayers)
	//检查房间是否已经满了
	if int32(sumPlayers) == r.maxPlayer {
		return false
	}

	//如果房间没有人，则进入房间成为房主,房主始终是准备状态
	if sumPlayers == 0{
		player.IsOwner = true
		player.Status = true
	}

	//修改玩家房主状态，以及加入该房间中
	r.RoomLock.Lock()
	player.RoomID = r.Rid
	r.RoomPlayers[player.Id] = player
	r.RoomLock.Unlock()
	return true
}


//退出房间-删除一个玩家
func (r *Room) DeletePlayer(uid string) {
	log.Println("[DeletePlayer Room]")
	if r == nil {
		log.Println("[DeletePlayer Room]，房间不存在")
		return
	}
	if _, ok := r.RoomPlayers[uid]; !ok {
		log.Println("退出房间失败，不存在该玩家")
		return
	}

	//判断退出的玩家是否是房主
	flag := r.RoomPlayers[uid].IsOwner
	//设置为非房主
	r.RoomPlayers[uid].IsOwner = false

	//从房间中删除
	r.RoomLock.Lock()
	delete(r.RoomPlayers,uid)
	r.RoomLock.Unlock()

	//如果退出的是房主，且房间中还有人，则更换房主
	if flag == true && len(r.RoomPlayers) >= 1{
		r.UpdateOwner()
	}
}

//更换房主
func(r *Room) UpdateOwner() {
	log.Println("[UpdateOwner Room]")

	//将所有人的房主属性设置为fasle

	//for _,player := range r.RoomPlayers {
	//	player.IsOwner = false
	//}

	//将第一个（map是key是hash 所以不是按插入顺序），等于随机选一个当房主
	r.RoomLock.Lock()
	for _,player := range r.RoomPlayers {
		player.Status = true
		player.IsOwner = true
		break
	}
	r.RoomLock.Unlock()
}


//在房间内广播消息(除掉发起者)
func (r *Room) BroadCast(MsgId uint32, data proto.Message, player *Player) {
	if r == nil {
		return
	}
	r.RoomLock.RLock()
	for _, v := range r.RoomPlayers{
		if v != player { //排除掉发起广播请求的玩家
			r.SendMsg(MsgId, data, v)
		}
	}
	r.RoomLock.RUnlock()
}

//在房间内广播消息(给所有人)
func (r *Room) BroadCastALL(MsgId uint32, data proto.Message) {
	if r == nil {
		return
	}
	r.RoomLock.RLock()
	for _, v := range r.RoomPlayers{
		r.SendMsg(MsgId, data, v)
	}
	r.RoomLock.RUnlock()
}

func (r *Room) SendMsg(MsgId uint32, data proto.Message, player *Player) {
	msg, _ := proto.Marshal(data)
	//调用框架的 SendMsg 方法
	if err := player.Conn.SendMsg(MsgId, msg); err != nil {
		log.Println("Room Player SendMsg error !")
		return
	}
}


//获取房间中所有玩家信息
func (r *Room) GetAllPlayer() []*Player {
	players := make([]*Player, 0)
	//加读锁
	r.RoomLock.RLock()
	for _, v := range r.RoomPlayers{
		players = append(players, v)
	}
	//解读锁
	r.RoomLock.RUnlock()
	return players
}