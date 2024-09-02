using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MsgBase 
{
    public string protoName = "";

    public MsgBase()
    {
        protoName = this.GetType().Name;
    }
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonUtility.ToJson(msgBase);
        return Encoding.UTF8.GetBytes(s);
    }

    public static MsgBase Decode(string protoName, byte[] bytes,int offset,int count)
    {
        string s = Encoding.UTF8.GetString(bytes, offset, count);
        return (MsgBase)JsonUtility.FromJson(s,Type.GetType(protoName));
    }

    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(msgBase.protoName);
        short len = (short)nameBytes.Length;
        byte[] bytes = new byte[len+2];

        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);

        Array.Copy(nameBytes,0,bytes,2,len);
        return bytes;
    }

    public static string DecodeName(byte[] bytes,int offset,out int count)
    {
        count = 0;
        if (offset + 2 > bytes.Length)
            return "";
        //short len = (short)(bytes[offset+1]*256+bytes[offset]);
        short len = (short)(bytes[offset+1]<<8 | bytes[offset]);
        if (len <= 0)
            return "";
        count = 2+len;
        return Encoding.UTF8.GetString(bytes, offset + 2, len);
    }
}
