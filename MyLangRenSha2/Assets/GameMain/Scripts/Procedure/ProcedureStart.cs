using GameFramework.Fsm;
using GameFramework.Procedure;
using System.Collections;
using System.Collections.Generic;


public class ProcedureStart : ProcedureBase
{


    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        //打开开始界面
        GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("Start/CanvasStart"), "Start");
    }

    protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnInit(procedureOwner);
        
    }
}
