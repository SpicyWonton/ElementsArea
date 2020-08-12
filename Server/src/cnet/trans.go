package cnet

import (
	"fmt"
	"inet"
	"io"
	"log"
	"time"
)

type Trans struct {
}

//提供一个Trans实例
func NewTrans() *Trans{
	tran := &Trans{}
	return tran
}


//读函数
func (trans *Trans) Reader(c*Connection) {
	log.Println("Reader goroutine is runing")
	defer c.Stop()
	cnt := 0
	for {
		//创建拆包解包的对象
		mp := NewPackMsg()

		//读取客户端的msg head
		headData := make([]byte, 8)
		if _,err := io.ReadFull(c.GetConn(), headData); err != nil {
			fmt.Println("read msg head err", err)
			break
		}

		//将msg head拆包，得到msgid和msglen
		msg, err := mp.Unpack(headData)
		if err != nil {
			fmt.Println("unpack error:", err)
			break
		}

		//根据datalen读取，读取剩下的data
		data := []byte {}
		data = make([]byte, msg.GetDataLen())
		if _, err := io.ReadFull(c.GetConn(),data); err != nil {
			log.Println("read msg data err:", err)
			break
		}
		msg.SetData(data)

		req := Req{
			conn: c,
			msg: msg,
		}
		//log.Printf("链接id:%d 第%d个包, 收到数据包的时间戳：%d\n",c.GetConnId(), cnt, time.Now().UnixNano() / 1e6)
		cnt++
		//message := make(chan byte)
		////心跳计时
		//go HeartBeating(req.GetConnection(), message, 20)
		////检测每次是否有数据传入
		//go GravelChannel(data, message)
		MgtRouterObj.SendMsgToTaskQueue(&req)
	}
}

//写函数
func (trans *Trans) Writer(c *Connection)  {
	log.Println("Writer goroutine is runing")
	defer log.Println(c.GetRemoteAddr().String(), " conn Writer exit!")
	defer c.Stop()
	//cnt := 0
	for {
		select {
		case data := <-c.msgChan:
			//log.Println("发送数据包之前时间戳：",time.Now().UnixNano() / 1e6)
			if _, err := c.Conn.Write(data); err != nil {
				log.Println("Send Data error:, ", err, " Conn Writer exit")
				return
			}
			//log.Println("发送数据包之后时间戳：",time.Now().UnixNano() / 1e6)
		case <- c.ExitChan:
			return
		}
	}
}

func GravelChannel(bytes []byte, mess chan byte) {
	for _, v := range bytes{
		mess <- v
	}
	close(mess)
}


func HeartBeating(conn inet.IConnection, bytes chan byte, timeout int) {
	select {
	case <- bytes:
		//log.Println(conn.GetConn().RemoteAddr().String(), "心跳:第", string(fk), "times")
		conn.GetConn().SetDeadline(time.Now().Add(time.Duration(timeout) * time.Second))
	}
}