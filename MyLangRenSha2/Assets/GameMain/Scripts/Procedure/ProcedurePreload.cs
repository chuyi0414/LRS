using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
using System.Collections.Generic;
using System.Text;


public class ProcedurePreload : ProcedureBase
{
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);

        ChangeState<ProcedureStart>(procedureOwner);
    }
}