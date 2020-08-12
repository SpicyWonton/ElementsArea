package inet

//服务器接口
type IServer interface {
	Start()                                 //启动服务器
	Stop()                                  //停止服务器
	AddRouter(msgId uint32, router IRouter) //添加路由

	SetLostConn(func (IConnection))			//设置链接断开前执行的函数
	LostConn(conn IConnection)				//链接断开前执行的函数
}
