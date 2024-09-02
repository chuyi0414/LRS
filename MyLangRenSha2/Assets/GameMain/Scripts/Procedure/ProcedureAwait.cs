using GameFramework.Entity;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;

public class ProcedureAwait : ProcedureBase
{
    protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnInit(procedureOwner);
        
        
    }

    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        GameEntry.MYNetManager.Connect();

        int RockerId = GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("Role/Rocker"), "Role");
        GameEntry.GameManager.RockerId = RockerId;

        GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("Hall/CanvasSetting"), "Hall");
    }
    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        base.OnLeave(procedureOwner, isShutdown);
    }
}
