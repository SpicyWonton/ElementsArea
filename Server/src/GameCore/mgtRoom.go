package GameCore

import (
	"fmt"
	"sync"
)

//管理房间的类
type MgtRoom struct {
	rooms map[int32]*Room
	roomLock sync.Mutex
}

//提供一个管理房间类的全局对象
var MgtRoomObj *MgtRoom

//自动加载init方法，房间列表进行初始化
func init() {
	MgtRoomObj = &MgtRoom{rooms:make(map[int32]*Room)}
}


//创建一个房间，并将创建房间的玩家添加进房间
func (mr *MgtRoom) CreateRoom(roomid int32, maxplayer int32, player *Player) bool{
	//fmt.Println("[CreateRoom MgtRoom]")

	//创建一个房间
	room := NewRoom(roomid,maxplayer)

	//将创建的玩家加入该房间
	player.RoomID = roomid 	//设置该玩家房号
	player.IsOwner = true 	//将该玩家设置为房主
	player.Status = true;	//设置玩家状态为在房间中准备
	room.AddPlayer(player)  //将玩家添加到房间中

	//添加房间加锁，避免房间id冲突
	mr.roomLock.Lock()
	if _,ok := mr.rooms[roomid]; ok {
		println("该房间id已存在")
		return false
	}
	mr.rooms[roomid] = room
	mr.roomLock.Unlock()

	return true
}



///根据id得到房间
func (mr *MgtRoom) GetRoomById(roomid int32) *Room{
	//fmt.Println("[GetRoomById MgtRoom]")
	if _, ok := mr.rooms[roomid]; !ok {
		fmt.Println("没有该房间")
	}
	return mr.rooms[roomid]
}


//离开房间
func (mr *MgtRoom) LeaveRoom(player *Player) {
	//fmt.Println("[LeaveRoom MgtRoom]")
	room := mr.rooms[player.RoomID]
	room.DeletePlayer(player.Id)

	//房间为空时，应该删除房间
	if len(room.RoomPlayers) == 0 {
		fmt.Println("房间为空，删除房间")
		delete(mr.rooms, player.RoomID)
	}
}

type Roominfo struct {
	Id int32	//房间id
	Nums int32	//房间人数
	OwnerName string //房主名称
}

//获取房间列表
func(mr *MgtRoom) GetRoomList() []Roominfo{
	//fmt.Println("[GetRoomList MgtRoom]")

	//顶一个房间信息的结构体数组
	infoArr := []Roominfo{}

	//遍历所有的房间(未在游戏中的房间)
	for _,v := range mr.rooms {
		//房间正在游戏中，就跳过
		if v.Status == true {
			continue
		}
		//查找该房间中的房主的id
		var ownerplay = &Player{}
		for _, ownerplay = range v.RoomPlayers {
			if ownerplay.IsOwner == true {
				break
			}
		}

		//房间信息
		roominfo := Roominfo{
			Id:     v.Rid,
			Nums:   int32(len(v.RoomPlayers)),
			OwnerName: ownerplay.Pmsg.Name,
		}

		//把房间信息添加到一个数组中
		infoArr = append(infoArr, roominfo)
	}
	return infoArr
}
