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
    //��ʼ��Ϸ�����ã��˳���Ϸ��ť
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
        //��ӿ�ʼ��Ϸ����
        mBtnStart.onClick.AddListener(OnBtnStart);
        mBtnSet.onClick.AddListener(OnBtnSet);

        
    }

    private void OnBtnSet()
    {

        
    }

    //��ʼ��Ϸ����
    private void OnBtnStart()
    {
        //�л�����
        GameFramework.Fsm.IFsm<IProcedureManager> procedureOwner = GameEntry.Fsm.GetFsm<IProcedureManager>();
        var Procedure = GameEntry.Procedure.CurrentProcedure;
        Procedure.ChangeState<ProcedureAwait>(procedureOwner);

        //�ر��Լ�
        GameEntry.UI.CloseUIForm(this.UIForm);

        
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        base.OnClose(isShutdown, userData);
       
    }
}
