using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public class Role : EntityLogic
{
    public float moveSpeed = 5f; // ÒÆ¶¯ËÙ¶È
    private Rigidbody2D rb;

    public uint guid;

    public void OnOpts(Opts opts)
    {
        switch(opts.operation)
        {
            case Operation.Joystick:
                Move(new Fixed64Vector2(opts.param[0], opts.param[1]));
                break;
            case Operation.MoveLocation:
                MoveLocation(new Fixed64Vector2(opts.param[0], opts.param[1]));
                break;
            default:
                return;
        }
    }
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        NetManager.AddMsgListener("MsgGetInfo", OnMsgGetInfo);
    }

    private void OnMsgGetInfo(MsgBase msgBase)
    {
        MsgGetInfo msgGetInfo = (MsgGetInfo)msgBase;
        guid = msgGetInfo.guid;
    }

    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public void MoveLocation(Fixed64Vector2 dir)
    {
        transform.position = new Vector2((float)dir.x, (float)dir.y);
    }

    public void Move(Fixed64Vector2 dir)
    {
        Vector2 movement = new Vector2((float)dir.x, (float)dir.y) * moveSpeed;
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }

}
