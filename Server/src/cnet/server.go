package cnet

import (
	"fmt"
	"gloconfig"
	"inet"
	"log"
	"net"
)

//Server服务类，实现IServer接口
type Server struct {
	Name      string //服务器名称
	IPVersion string //ipv4 或ipv6
	IP        string //服务器绑定的ip地址
	Port      int    //服务器绑定的端口
	//mgtRouter inet.IMgtRouter 	//路由管理器
	OnConnStop func(conn inet.IConnection) //链接断开前执行的函数
}

//提供一个Server实例
func New() inet.IServer {
	s := &Server{
		Name:      gloconfig.ConfigObj.Name,
		IPVersion: "tcp4",
		IP:        gloconfig.ConfigObj.IP,
		Port:      gloconfig.ConfigObj.Port,
		//mgtRouter: NewMgtRouter(),
	}
	return s
}

//>>>>>>>>>>>>>>>>>>>>>>>>>>>一下实现iserver接口的方法

//开始运行服务器
func (s *Server) Start() {

	log.Printf("START Server listenner IP: %s, Port: %d\n", s.IP, s.Port)

	//开启goroutine 去做listen
	go func() {
		//启动worker工作池机制
		log.Println("启动工作池")
		MgtRouterObj.StartWorkerPool()

		//获取tcp的Addr
		addr, err := net.ResolveTCPAddr(s.IPVersion, fmt.Sprintf("%s:%d", s.IP, s.Port))
		if err != nil {
			log.Fatalln("ResolveTCPAddr err:", err)
		}

		//监听服务器地址
		listen, err := net.ListenTCP(s.IPVersion, addr)
		if err != nil {
			log.Fatalln("ListenTCP err:", err)
		}

		//给链接分配的链接id
		var cid uint32 = 0
		//等待链接
		for {
			//阻塞等待连接
			conn, err := listen.AcceptTCP()
			if err != nil {
				log.Println("AcceptTCP err:", err)
				//接受链接失败，继续接收
				continue
			}

			//创建一个链接实例
			dealConn := Newconn(s, conn, cid)
			cid++

			//启动该链接的读写业务
			go dealConn.Start()
		}

	}()

	//阻塞，不然上面的go会退出
	select {}
}

//停止运行服务器
func (s *Server) Stop() {
	log.Println("STOP server")
}

//添加路由
func (s *Server) AddRouter(msgId uint32, router inet.IRouter) {
	log.Printf("AddRouter msgId = %d\n", msgId)
	MgtRouterObj.AddRouter(msgId, router)
}

//设置断开链接前的函数
func (s *Server) SetLostConn(hookFunc func(inet.IConnection)) {
	log.Println("SetLostConn")
	s.OnConnStop = hookFunc
}

//执行断开链接前的函数
func (s *Server) LostConn(conn inet.IConnection) {
	log.Println("LostConn")
	s.OnConnStop(conn)
}
