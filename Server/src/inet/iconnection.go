package inet

import "net"

type IConnection interface {
	Start() 					//启动链接
	Stop()						//停止链接
	GetConn() *net.TCPConn		//获取原始scoket链接
	GetConnId() uint32			//获取链接id
	GetRemoteAddr() net.Addr	//获取远程客户端Addr
	SendMsg(msgId uint32, data []byte) error	//发送消息给客户端

	SetProperty(key string, value interface{}) //设置链接属性
	GetProperty(key string)(interface{}, error) //获取链接属性
	RemoveProperty(key string) //移除链接属性
}
