using GameFramework.Procedure;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using Cinemachine;
using GameFramework.Event;
using System;

public class CanvasStart : UIFormLogic
{
    //开始游戏，设置，退出游戏按钮
    public Button mBtnStart;
    public Button mBtnSet;
    public Button mBtnEnd;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
               
    }


    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        //添加开始游戏方法
        mBtnStart.onClick.AddListener(OnBtnStart);
        mBtnSet.onClick.AddListener(OnBtnSet);

        
    }

    private void OnBtnSet()
    {

        
    }

    //开始游戏方法
    private void OnBtnStart()
    {
        //切换流程
        GameFramework.Fsm.IFsm<IProcedureManager> procedureOwner = GameEntry.Fsm.GetFsm<IProcedureManager>();
        var Procedure = GameEntry.Procedure.CurrentProcedure;
        Procedure.ChangeState<ProcedureAwait>(procedureOwner);

        //关闭自己
        GameEntry.UI.CloseUIForm(this.UIForm);

        
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        base.OnClose(isShutdown, userData);
       
    }
}
