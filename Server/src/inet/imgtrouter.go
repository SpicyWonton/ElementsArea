package inet

type IMgtRouter interface {
	AddRouter(msgId uint32, router IRouter) //为消息添加具体的处理逻辑
	DoMsgHandler(request IReq)

	StartWorkerPool()                   //启动worker工作池
	SendMsgToTaskQueue(request IReq)    //将消息交给TaskQueue,由worker进行处理
}
