using UnityEngine;
using UnityGameFramework.Runtime;

/// <summary>
/// 游戏入口。
/// </summary>
public partial class GameEntry : MonoBehaviour
{
    public static NetManager MYNetManager;
    public static GameManager GameManager;

    private static void InitCustomComponents()
    {
        // 将来在这里注册自定义的组件
        MYNetManager = UnityGameFramework.Runtime.GameEntry.GetComponent<NetManager>();
        GameManager = UnityGameFramework.Runtime.GameEntry.GetComponent<GameManager>();
    }

    private static void InitCustomDebuggers()
    {
        // 将来在这里注册自定义的调试器
    }
}