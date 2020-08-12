package inet

//IReq接口，把请求链接和数据封装到了Req中

type IReq interface {
	GetConnection() IConnection //获取请求连接信息
	GetData() []byte            //获取请求消息的数据
	GetMsgID() uint32			//获取消息ID
}