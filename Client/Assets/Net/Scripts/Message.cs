/*
    消息类    
*/
public class Message
{
    public uint id;
    public uint dataLen;
    public byte[] data;

    public Message()
    {

    }

    public Message(uint id, uint dataLen, byte[] data)
    {
        this.id = id;
        this.dataLen = dataLen;
        this.data = data;
    }
}
