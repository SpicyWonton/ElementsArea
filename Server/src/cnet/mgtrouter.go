package cnet

import (
	"GameCore"
	"gloconfig"
	"inet"
	"log"
	"pbf"
)

type MgtRouter struct {
	Api            map[uint32]inet.IRouter //MsgId 对应 router
	WorkerPoolSize uint32                  //业务工作Worker池的数量
	TaskQueue      []chan inet.IReq        //Worker负责取任务的消息队列
	ExitQueue      []chan bool             //负责关闭worker的管道
}

func init() {
	MgtRouterObj = &MgtRouter{
		Api:            make(map[uint32]inet.IRouter),
		WorkerPoolSize: uint32(gloconfig.ConfigObj.Poolsize),
		TaskQueue:      make([]chan inet.IReq, 1000000), //一个worker对应一个queue
		ExitQueue:      make([]chan bool, 1000000),
	}
}

//开一个全局路由对象
var MgtRouterObj *MgtRouter

func (mr *MgtRouter) AddRouter(msgId uint32, router inet.IRouter) {
	mr.Api[msgId] = router
}

//以非阻塞的方法处理具体router
func (mh *MgtRouter) DoMsgHandler(request inet.IReq) {
	handler, ok := mh.Api[request.GetMsgID()]

	//判断该router是否存在
	if !ok {
		log.Println("api msgId = ", request.GetMsgID(), " is not FOUND!")
		return
	}
	//执行对应的方法
	handler.Deal(request)
}

//启动一个Worker工作流程
func (mh *MgtRouter) StartOneWorker(workerID int, taskQueue chan inet.IReq) {
	//不断的等待队列中的消息
	for {
		select {
		case <-mh.ExitQueue[workerID]:
			return
		//有消息则取出队列的Request，并执行绑定的业务方法
		case request := <-taskQueue:
			mh.DoMsgHandler(request)
		}
	}
}

//启动worker工作池
func (mh *MgtRouter) StartWorkerPool() {
	//遍历需要启动worker的数量，依此启动
	for i := 0; i < int(mh.WorkerPoolSize); i++ {
		//一个worker被启动
		//给当前worker对应的任务队列开辟空间
		mh.TaskQueue[i] = make(chan inet.IReq, gloconfig.ConfigObj.TaskLen)
		//启动当前Worker，阻塞的等待对应的任务队列是否有消息传递进来
		go mh.StartOneWorker(i, mh.TaskQueue[i])
	}
}

//将消息交给TaskQueue,由worker进行处理
func (mh *MgtRouter) SendMsgToTaskQueue(request inet.IReq) {
	//根据ConnID来分配当前的连接应该由哪个worker负责处理
	var workerID uint32 = 0

	//获取msgid
	msgid := request.GetMsgID()

	//开始游戏，另外开一个go和消息队列处理这个房间的请求
	if msgid == uint32(pbf.MSGID_STARTGAME) {
		//获取链接属性
		pid, err := request.GetConnection().GetProperty("pid")
		if err != nil {
			log.Println("sendmsgtotaskqueue GetProperty err:", err)
			return
		}

		//获取玩家
		player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))

		//实例化一个新的消息队列
		mh.TaskQueue[player.RoomID+100] = make(chan inet.IReq, gloconfig.ConfigObj.TaskLen)
		mh.ExitQueue[player.RoomID+100] = make(chan bool, 1)
		//开启一个go，做这个房间的消息
		go mh.StartOneWorker(int(player.RoomID+100), mh.TaskQueue[player.RoomID+100])
		workerID = uint32(player.RoomID + 100)

	} else if msgid >= uint32(pbf.MSGID_LOADMAP) && msgid <= uint32(pbf.MSGID_ADDSCORE) { //在局内的玩家请求
		//获取链接属性
		pid, err := request.GetConnection().GetProperty("pid")
		if err != nil {
			log.Println("sendmsgtotaskqueue GetProperty err:", err)
			return
		}

		//获取玩家
		player := GameCore.MgtplayerObj.GetPlayerById(pid.(string))
		if player.RoomID == 0 && msgid == uint32(pbf.MSGID_SYNCPHV) {
			player.SendMsg(uint32(pbf.MSGID_BROSYNCPHV), &pbf.BroadCastMove{})
			log.Println("发送")
			return
		}
		workerID = uint32(player.RoomID + 100)
	} else {
		//非局内的请求，采用取余分配到不同的消息队列
		workerID = request.GetConnection().GetConnId() % uint32(gloconfig.ConfigObj.Poolsize)
	}
	//将请求消息发送给任务队列
	mh.TaskQueue[workerID] <- request
}
