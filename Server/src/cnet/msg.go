package cnet

type Msg struct {
	DataLen uint32 //消息的长度
	Id uint32	   //消息的Id
	Data []byte	   //消息的容
}

//提供一个Msg实例
func NewMsg(id uint32, data []byte) *Msg {
	return &Msg{
		DataLen: uint32(len(data)),
		Id: id,
		Data: data,
	}
}

//获取消息数据段长度
func (msg *Msg) GetDataLen() uint32 {
	return msg.DataLen
}

//获取消息ID
func (msg *Msg) GetMsgId() uint32 {
	return msg.Id
}

//获取消息内容
func (msg *Msg) GetData() []byte {
	return msg.Data
}

//设置消息数据段长度
func (msg *Msg) SetDataLen(len uint32) {
	msg.DataLen = len
}

//设计消息ID
func (msg *Msg) SetMsgId(msgId uint32) {
	msg.Id = msgId
}

//设计消息内容
func (msg *Msg) SetData(data []byte) {
	msg.Data = data
}
