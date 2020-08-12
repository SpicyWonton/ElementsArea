package gloconfig

import (
	inet2 "inet"
)

//存储一些全局参数，供用户配置和其它模块使用
type Config struct {
	Server inet2.IServer //当前的全局Server对象
	Name   string        //服务器名称
	IP     string        //服务器绑定的ip地址
	Port   int           //服务器绑定的端口
	Poolsize int		//工作池数量
	TaskLen int			//工作池对应的消息队列长度
}

//提供一个全局对象
var ConfigObj *Config

//提供init方法，自动加载
func init() {
	ConfigObj = &Config{
		Name: "g2 server",
		IP:   "172.20.80.52",
		Port: 7777,
		Poolsize : 10,
		TaskLen : 1024,
	}
}