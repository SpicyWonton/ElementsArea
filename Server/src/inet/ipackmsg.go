package inet

type IDataPack interface{
	Pack(msg IMsg)([]byte, error) //封包
	Unpack([]byte)(IMsg, error)   //拆包
}