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

        // TODO: ������Բ���һ�� Splash ����
        // ...

        if (GameEntry.Base.EditorResourceMode)
        {
            // �༭��ģʽ
            Log.Info("�༭��ģʽ.");
            ChangeState<ProcedurePreload>(procedureOwner);
        }
        else if (GameEntry.Resource.ResourceMode == ResourceMode.Package)
        {
            // ����ģʽ
            Log.Info("����ģʽ.");
            ChangeState<ProcedureInitResources>(procedureOwner);
        }
        else
        {
            // �ɸ���ģʽ
            Log.Info("�ɸ���ģʽ.");
            ChangeState<ProcedureCheckVersion>(procedureOwner);
        }
    }
}