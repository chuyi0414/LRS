using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;


public class RoleRocker : UIFormLogic
{
    public int currTuen;
    //摇杆
    public FixedJoystick joystick;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        NetManager.AddMsgListener("MsgLockStepBack", OnMsgLockStepBack);
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
    }

    private void OnMsgLockStepBack(MsgBase msgBase)
    {
        MsgLockStepBack msgLockStepBack = (MsgLockStepBack)msgBase;

        if (msgLockStepBack.turn < currTuen)
            return;

        for(int i=0; i<msgLockStepBack.unsyncOpts.Length;i++)
        {
            OnOpts(msgLockStepBack.unsyncOpts[i]);
        }
        currTuen = msgLockStepBack.turn;

        MsgLockStep msg =new MsgLockStep();
        msg.turn = currTuen+1;

        List <Opts> opts = new List<Opts>()
        {
            GetJoystick()
        };
        msg.opts = opts.ToArray();
        NetManager.SendTo(msg, NetManager.ServerType.Fighter);
    }

    private void OnOpts(UnsyncOpts unsyncOpts)
    {
        for (int i = 0;i<unsyncOpts.opts.Length;i++)
        {
            foreach(var item in Roles.Instance.mRoles)
            {
                if(item.Key == unsyncOpts.opts[i].guid)
                {
                    item.Value.OnOpts(unsyncOpts.opts[i]);
                }
            }
        }
    }

    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        if(!GameEntry.Setting.GetBool("isZTB") && Roles.Instance.mRoles.ContainsKey(GameEntry.GameManager.guid))
        {
            Opts opts = GetJoystick();
            Roles.Instance.mRoles[GameEntry.GameManager.guid].Move(new Fixed64Vector2(opts.param[0], opts.param[1]));
            
        }

    }

    public Opts GetJoystick()
    {
        Opts opts = new Opts();
        opts.operation = Operation.Joystick;
        opts.param = new Fixed64[2];
        float v = joystick.Vertical;
        float h = joystick.Horizontal;
        opts.param[0] = (Fixed64)h;
        opts.param[1] = (Fixed64)v;

        opts.guid = GameEntry.GameManager.guid;
        return opts;
    }
}