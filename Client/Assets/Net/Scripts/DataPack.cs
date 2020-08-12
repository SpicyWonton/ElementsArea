/*
    提供将消息封包和拆包的方法 
*/
using System;

public class DataPack
{
    // 将消息类封包成字节数组
    public static byte[] Pack(Message msg)
    {
        // 将消息类中的成员变量转换为字节类型
        byte[] id = BitConverter.GetBytes(msg.id);
        byte[] dataLen = BitConverter.GetBytes(msg.dataLen);
        // 最终封包的结果
        byte[] binaryData = new byte[id.Length + dataLen.Length + msg.data.Length];
        // 合并三个数组
        Array.Copy(dataLen, 0, binaryData, 0, dataLen.Length);
        Array.Copy(id, 0, binaryData, dataLen.Length, id.Length);
        Array.Copy(msg.data, 0, binaryData, id.Length + dataLen.Length, msg.data.Length);

        return binaryData;
    }

    // 将字节数组拆包成消息类，不包含数据
    public static Message UnPack(byte[] headData)
    {
        // 最终拆包结果
        Message msg = new Message();
        // 前四个字节是消息数据的长度
        uint dataLen = BitConverter.ToUInt32(headData, 0);
        // 接下来四个字节是消息ID
        uint id = BitConverter.ToUInt32(headData, 4);
        // 赋值
        msg.dataLen = dataLen;
        msg.id = id;

        return msg;
    }
}
