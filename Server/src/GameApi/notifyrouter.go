package GameApi

import (
	"GameCore"
	"cnet"
	"fmt"
	"inet"
	"log"
	"pbf"
	"sort"
	"time"
)

//玩家通知已经加载完地图
type LoadFinishedRouter struct {
	cnet.BaseRouter
}
func(sg *LoadFinishedRouter) Deal(req inet.IReq) {
	log.Println("LoadFinishedRouter")

	pid, _ := req.GetConnection().GetProperty("pid")
	player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

	//获取房间
	room := GameCore.MgtRoomObj.GetRoomById(player.RoomID)
	//加载玩家数量+1
	room.LoadFin = room.LoadFin + 1

	//确保所有玩家加载完地图，再开始游戏
	if room.LoadFin == int32(len(room.RoomPlayers)) {
		msgdata := &pbf.GameTime{
			TimeLeft: 120,
		}
		//加载完成状态清零
		room.LoadFin = 0
		//在房间内广播开始游戏的消息
		room.BroadCastALL(uint32(pbf.MSGID_BROENTERGAME),msgdata)

		//获取开始游戏的时间戳
		now:= time.Now()
		room.StartTime = int32(now.Unix())

		go Ending(room)
		go SyncPHV(room)
	}
}

func Ending(room *GameCore.Room) {
	//固定时间后，游戏结束
	time.Sleep(time.Second * 120)

	room.GAMEEXIT <- true

	Settlement(room)

	cnet.MgtRouterObj.ExitQueue[room.Rid+100] <- true
}


func SyncPHV(room *GameCore.Room) {
	//定时的发送玩家的位置
	for {
		select {
			case <- room.GAMEEXIT:
				close(room.GAMEEXIT)
				return
			default:
				players := room.GetAllPlayer()

				msgdata := &pbf.BroadCastMove{
					PlayerInfo: nil,
				}

				for _, v := range players {
					infolist := &pbf.Player{
						Pos:   v.P,
						Rot:   v.R,
						Speed: v.Speed,
						Pid:   v.StartId,
						HP:	   v.Hp,
					}
					msgdata.PlayerInfo = append(msgdata.PlayerInfo, infolist)
				}
				room.BroadCastALL(uint32(pbf.MSGID_BROSYNCPHV), msgdata)
				time.Sleep(time.Millisecond * 66)
				//fmt.Println("发送位置")
			}
	}
}

//游戏结算
type ScoreSett struct {
	Socre int
	Pid int
}

func Settlement(room *GameCore.Room) {
	//fmt.Println("发送了")

	players := room.GetAllPlayer()
	//找到最高得分
	maxsocre := 0

	//将玩家得分赋值给一个结构体
	s := make([]ScoreSett, 6)
	for i,t := range room.Score {
		s[i] = ScoreSett{Socre: t, Pid: i}
	}
	//将玩家得分进行排序
	sort.Slice(s, func(i, j int) bool {
		if s[i].Socre > s[j].Socre {
			return true
		}
		return false
	})

	rankexp := make([]int32, 6)
	rankexp[0] = 50
	rankexp[1] = 30
	rankexp[2] = 10

	data := &pbf.GameSettleArr{}

	//遍历房间玩家列表
	for _,v := range players {

		v.Pmsg.Score += int32(room.Score[v.StartId])
		var addexp int32 = 0
		for i, t := range s {
			if int32(t.Pid )== v.StartId {
				addexp = rankexp[i] + int32(maxsocre) * 10
				v.Pmsg.Experience +=  addexp
				break
			}
		}
		//fmt.Println("发送了")
		//计算经验对应等级，更新经验和等级
		for v.Pmsg.Experience >= (v.Pmsg.Rank + 1) * 50 {
			v.Pmsg.Experience -= (v.Pmsg.Rank + 1) * 50
			v.Pmsg.Rank += 1
		}

		//给玩家发送等级和经验
		tdata := &pbf.GameSettle{
			UID:      v.Id,
			Name:     v.Pmsg.Name,
			Score:    v.Pmsg.Score * 50,
			EXP:      v.Pmsg.Experience,
			Rank:     v.Pmsg.Rank,
			HeadIcon: v.Pmsg.HeadIcon,
			AddEXP:	  addexp,
		}
		data.AllData = append(data.AllData, tdata)
		v.Status = false
		v.IsOwner = false
		v.Pmsg.Score = 0
	}
	//fmt.Println("发送了")
	room.BroadCastALL(uint32(pbf.MSGID_GAMEFIN), data)
	for _,v := range players {
		GameCore.MgtRoomObj.LeaveRoom(v)
		v.RoomID = 0
		v.Status = false
	}
	fmt.Println("将玩家都移除处房间了")
}



