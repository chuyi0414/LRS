using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class cs 
{
    // Start is called before the first frame update
    void Start()
    {
        //NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnEventConnectSucc);

        NetManager.AddMsgListener("MsgTest", OnMsgTest);
        NetManager.Connect("127.0.0.1",8888);

        //Invoke("Test",2);
    }

    private void Test()
    {
        MsgTest msgTest = new MsgTest();
        NetManager.Send(msgTest,NetManager.ServerType.Fighter);
    }

    private void OnMsgTest(MsgBase msgBase)
    {
        Debug.Log("�ͻ����յ�");
    }

    private void OnEventConnectSucc(string err)
    {
        Debug.Log("���ӳɹ�");
    }

    // Update is called once per frame
    void Update()
    {
        //NetManager.Update();
    }
}
