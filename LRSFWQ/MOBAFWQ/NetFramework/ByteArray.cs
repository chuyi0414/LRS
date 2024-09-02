using System;
using System.Collections;
using System.Collections.Generic;

public class ByteArray
{
    /// <summary>
    /// Ĭ�ϳ���
    /// </summary>
    const int DEFAULT_SIZE = 1024;
    /// <summary>
    /// �ֽ�����
    /// </summary>
    public byte[] bytes;
    /// <summary>
    /// ����λ��
    /// </summary>
    public int readIndex;
    /// <summary>
    /// д��λ��
    /// </summary>
    public int writeIndex;
    /// <summary>
    /// ��ʼ��С
    /// </summary>
    public int initSize;
    /// <summary>
    /// ��������
    /// </summary>
    public int capacity;
    /// <summary>
    /// ��д֮��ĳ���
    /// </summary>
    public int Length { get { return writeIndex - readIndex; } }
    /// <summary>
    /// ����
    /// </summary>
    public int Remain { get { return capacity - writeIndex; } }
    /// <summary>
    /// �����ֽ�����
    /// </summary>
    /// <param name="size">�ֽ�����ĳ���</param>
    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        initSize = size;
        capacity = size;
        readIndex = 0;
        writeIndex = 0;
    }

    /// <summary>
    /// �����ֽ�����
    /// </summary>
    /// <param name="defaultBytes">Ĭ�ϵ��ֽ�����</param>
    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        initSize = defaultBytes.Length;
        capacity = defaultBytes.Length;
        readIndex = 0;
        writeIndex = defaultBytes.Length;
    }
    /// <summary>
    /// �ƶ�����
    /// </summary>
    public void MoveBytes()
    {
        if (Length > 0)
        {
            Array.Copy(bytes, readIndex, bytes, 0, Length);
        }
        writeIndex = Length;
        readIndex = 0;
    }
    /// <summary>
    /// ����
    /// </summary>
    public void ReSize(int size)
    {
        if (size < Length)
            return;
        if (size < initSize)
            return;
        capacity = size;
        //�³��ȵ�����
        byte[] newBytes = new byte[capacity];
        //��ԭ�������ݿ����������鵱��
        Array.Copy(bytes, readIndex, newBytes, 0, Length);
        bytes = newBytes;
        writeIndex = Length;
        readIndex = 0;
    }
}
