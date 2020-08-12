package main

import (
	"GameApi"
	"cnet"
	"log"
)

//func init() {
//	file := "./" + "message" + ".txt"
//	logFile, err := os.OpenFile(file, os.O_RDWR | os.O_CREATE | os.O_APPEND, 0766)
//	if err != nil {
//		panic(err)
//	}
//	log.SetOutput(logFile)
//	//return
//}

func main() {

	log.SetFlags(log.Ldate|log.Lshortfile)

	//创建一个Server
	S := cnet.New()

	//绑定需要的路由
	GameApi.AddAllRouter(S)

	//启动Server
	S.Start()
}