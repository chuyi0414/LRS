using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
#nullable disable
public class MsgBase
{
    public MsgBase()
    {
        protoName = this.GetType().Name;
    }
    /// <summary>
    /// 协议名
    /// </summary>
    public string protoName = "";
    /// <summary>
    /// 编码
    /// </summary>
    /// <param name="msgBase">消息</param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonConvert.SerializeObject(msgBase);
        return Encoding.UTF8.GetBytes(s);
    }
    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="protoName">协议名</param>
    /// <param name="bytes">字节数组</param>
    /// <param name="offset">数组起始位置</param>
    /// <param name="count">长度</param>
    /// <returns></returns>
    public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string s="";
        try
        {
            // 将字节数组转换为 JSON 字符串
            s = Encoding.UTF8.GetString(bytes, offset, count);
            // 获取目标类型
            Type type = Type.GetType(protoName);
            if (type == null)
            {
                Console.WriteLine($"S:{s}");
                Console.WriteLine($"无法找到类型: {protoName}");
                return null;
            }

            // 反序列化 JSON 字符串
            return (MsgBase)JsonConvert.DeserializeObject(s, type);
        }
        catch (JsonSerializationException ex)
        {
            Console.WriteLine($"S:{s}");
            Console.WriteLine($"JSON 反序列化错误: {ex.Message}\n");
            return null; // 返回 null 表示反序列化失败
        }
        catch (Exception ex)
        {
            Console.WriteLine($"S:{s}");
            Console.WriteLine($"一般错误: {ex.Message}");
            return null; // 返回 null 表示发生了一般错误
        }
    }

    /// <summary>
    /// 协议名编码
    /// </summary>
    /// <param name="msgBase">消息</param>
    /// <returns></returns>
    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(msgBase.protoName);
        short len = (short)nameBytes.Length;
        byte[] bytes = new byte[len + 2];

        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);

        Array.Copy(nameBytes, 0, bytes, 2, len);
        return bytes;
    }
    /// <summary>
    /// 协议名解码
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <param name="offset">起始位置</param>
    /// <param name="count">要返回的解析出来的长度</param>
    /// <returns></returns>
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        if (offset + 2 > bytes.Length)
            return "";
        short len = (short)(bytes[offset + 1] * 256 + bytes[offset]);
        if (len <= 0)
            return "";
        count = 2 + len;
        return Encoding.UTF8.GetString(bytes, offset + 2, len);
    }
}
