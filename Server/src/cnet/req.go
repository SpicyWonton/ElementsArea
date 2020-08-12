package cnet

import (
	"inet"
)
type Req struct {
	conn inet.IConnection //链接
	msg  inet.IMsg
}

//>>>>>>>>>>>>>>>>>>>>>>>>>>>一下实现ireq接口的方法

//获取请求连接信息
func(r *Req) GetConnection() inet.IConnection {
	return r.conn
}

//获取请求消息的数据
func(r *Req) GetData() []byte {
	return r.msg.GetData()
}

func(r *Req) GetMsgID() uint32 {
	return r.msg.GetMsgId()
}
