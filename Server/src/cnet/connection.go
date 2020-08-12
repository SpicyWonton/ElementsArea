package cnet

import (
	"errors"
	"inet"
	"log"
	"net"
	"sync"
)

//链接类，管理链接

type Connection struct {
	TcpServer inet.IServer 				//当前链接属于那个Server
	Conn      *net.TCPConn     			//当前链接的socket
	ConnId    uint32           			//当前连接的id
	IsClose   bool             			//当前链接是否关闭了
	msgChan   chan []byte				//读写数据管道
	ExitChan  chan bool 				//告知链接关闭的 channel
	property  map[string]interface{} 	//链接属性
	propertyLock sync.RWMutex 			//保护链接属性的读写锁
}

//提供一个Connection实例
func Newconn(server inet.IServer, conn *net.TCPConn, connId uint32) *Connection{
	c := &Connection{
		TcpServer: server,
		Conn:     conn,
		ConnId:   connId,
		IsClose:  false,
		ExitChan: make(chan bool, 1),
		msgChan:make(chan []byte),
		property:make(map[string]interface{}),
	}
	return  c
}

//>>>>>>>>>>>>>>>>>>>>>>>>>>>以下实现iserver接口的方法

//启动链接的读写go
func (c *Connection) Start(){
	trans := NewTrans()
	go trans.Reader(c)
	go trans.Writer(c)
	for {
		select {
		case <- c.ExitChan:
			return //得到退出消息，不再阻塞
		}
	}
}

//停止链接
func (c *Connection) Stop(){
	//判断链接是否已经关闭
	if c.IsClose == true {
		return
	}
	c.IsClose = true

	//执行关闭链接前的处理函数
	c.TcpServer.LostConn(c)

	//关闭socket连接
	err := c.Conn.Close()
	if err != nil {
		log.Println("c.Conn.Close() err:",err)
	}

	c.ExitChan <- true	//通知从换从队列读数据的业务，该连接已经关闭
	close(c.ExitChan)	//关闭该链接的全部通道
}


//获取原始scoket链接
func (c *Connection)GetConn() *net.TCPConn {
	return c.Conn
}

//获取链接id
func (c *Connection)GetConnId() uint32 {
	return c.ConnId
}

//获取远程客户端Addr
func (c *Connection)GetRemoteAddr() net.Addr {
	return c.Conn.RemoteAddr()
}

//直接将Message数据发送数据给远程的TCP客户端
func (c *Connection) SendMsg(msgId uint32, data []byte) error {
	if c.IsClose == true {
		return errors.New("Connection closed when send msg")
	}

	//将data封包，并且发送
	dp := NewPackMsg()
	msg, err := dp.Pack(NewMsg(msgId, data))
	if err != nil {
		log.Println("Pack error err:", err)
		return  errors.New("Pack error msg ")
	}

	//写回客户端
	c.msgChan <- msg

	return nil
}

//设置链接属性
func (c *Connection) SetProperty(key string, value interface{}) {
	c.propertyLock.Lock()
	defer c.propertyLock.Unlock()
	c.property[key] = value
}

//获取链接属性
func (c *Connection) GetProperty(key string) (interface{}, error) {
	if value, ok := c.property[key]; ok  {
		return value, nil
	} else {
		return nil, errors.New("no property found")
	}
}

//移除链接属性
func (c *Connection) RemoveProperty(key string) {
	c.propertyLock.Lock()
	defer c.propertyLock.Unlock()
	delete(c.property, key)
}

