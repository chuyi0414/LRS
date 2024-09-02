using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityGameFramework.Runtime;

public class ProcedureInit : ProcedureBase
{
    public  bool UseNativeDialog
    {
        get
        {
            return true;
        }
    }

    protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

        // TODO: 这里可以播放一个 Splash 动画
        // ...

        if (GameEntry.Base.EditorResourceMode)
        {
            // 编辑器模式
            Log.Info("编辑器模式.");
            ChangeState<ProcedurePreload>(procedureOwner);
        }
        else if (GameEntry.Resource.ResourceMode == ResourceMode.Package)
        {
            // 单机模式
            Log.Info("单机模式.");
            ChangeState<ProcedureInitResources>(procedureOwner);
        }
        else
        {
            // 可更新模式
            Log.Info("可更新模式.");
            ChangeState<ProcedureCheckVersion>(procedureOwner);
        }
    }
}