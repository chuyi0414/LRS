using UnityEngine;

using UnityGameFramework.Runtime;

/// <summary>
/// 游戏入口。
/// </summary>
public partial class GameEntry : MonoBehaviour
{
    private static BaseComponent _base;

    /// <summary>  
    /// 获取游戏基础组件。  
    /// </summary>  
    public static BaseComponent Base
    {
        get
        {
            if (_base == null)
            {
                _base = UnityGameFramework.Runtime.GameEntry.GetComponent<BaseComponent>();
            }
            return _base;
        }
        private set { _base = value; }
    }

    private static ConfigComponent _config;

    /// <summary>  
    /// 获取配置组件。  
    /// </summary>  
    public static ConfigComponent Config
    {
        get
        {
            if (_config == null)
            {
                _config = UnityGameFramework.Runtime.GameEntry.GetComponent<ConfigComponent>();
            }
            return _config;
        }
        private set { _config = value; }
    }

    private static DataNodeComponent _dataNode;

    /// <summary>  
    /// 获取数据结点组件。  
    /// </summary>  
    public static DataNodeComponent DataNode
    {
        get
        {
            if (_dataNode == null)
            {
                _dataNode = UnityGameFramework.Runtime.GameEntry.GetComponent<DataNodeComponent>();
            }
            return _dataNode;
        }
        private set { _dataNode = value; }
    }

    private static DataTableComponent _dataTable;

    /// <summary>  
    /// 获取数据表组件。  
    /// </summary>  
    public static DataTableComponent DataTable
    {
        get
        {
            if (_dataTable == null)
            {
                _dataTable = UnityGameFramework.Runtime.GameEntry.GetComponent<DataTableComponent>();
            }
            return _dataTable;
        }
        private set { _dataTable = value; }
    }

    private static DebuggerComponent _debugger;

    /// <summary>  
    /// 获取调试组件。  
    /// </summary>  
    public static DebuggerComponent Debugger
    {
        get
        {
            if (_debugger == null)
            {
                _debugger = UnityGameFramework.Runtime.GameEntry.GetComponent<DebuggerComponent>();
            }
            return _debugger;
        }
        private set { _debugger = value; }
    }

    private static DownloadComponent _download;

    /// <summary>  
    /// 获取下载组件。  
    /// </summary>  
    public static DownloadComponent Download
    {
        get
        {
            if (_download == null)
            {
                _download = UnityGameFramework.Runtime.GameEntry.GetComponent<DownloadComponent>();
            }
            return _download;
        }
        private set { _download = value; }
    }

    private static EntityComponent _entity;

    /// <summary>  
    /// 获取实体组件。  
    /// </summary>  
    public static EntityComponent Entity
    {
        get
        {
            if (_entity == null)
            {
                _entity = UnityGameFramework.Runtime.GameEntry.GetComponent<EntityComponent>();
            }
            return _entity;
        }
        private set { _entity = value; }
    }

    private static EventComponent _event;

    /// <summary>  
    /// 获取事件组件。  
    /// </summary>  
    public static EventComponent Event
    {
        get
        {
            if (_event == null)
            {
                _event = UnityGameFramework.Runtime.GameEntry.GetComponent<EventComponent>();
            }
            return _event;
        }
        private set { _event = value; }
    }

    private static FileSystemComponent _fileSystem;

    /// <summary>  
    /// 获取文件系统组件。  
    /// </summary>  
    public static FileSystemComponent FileSystem
    {
        get
        {
            if (_fileSystem == null)
            {
                _fileSystem = UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
            }
            return _fileSystem;
        }
        private set { _fileSystem = value; }
    }

    private static FsmComponent _fsm;

    /// <summary>  
    /// 获取有限状态机组件。  
    /// </summary>  
    public static FsmComponent Fsm
    {
        get
        {
            if (_fsm == null)
            {
                _fsm = UnityGameFramework.Runtime.GameEntry.GetComponent<FsmComponent>();
            }
            return _fsm;
        }
        private set { _fsm = value; }
    }

    private static LocalizationComponent _localization;

    /// <summary>  
    /// 获取本地化组件。  
    /// </summary>  
    public static LocalizationComponent Localization
    {
        get
        {
            if (_localization == null)
            {
                _localization = UnityGameFramework.Runtime.GameEntry.GetComponent<LocalizationComponent>();
            }
            return _localization;
        }
        private set { _localization = value; }
    }

    private static NetworkComponent _network;

    /// <summary>  
    /// 获取网络组件。  
    /// </summary>  
    public static NetworkComponent Network
    {
        get
        {
            if (_network == null)
            {
                _network = UnityGameFramework.Runtime.GameEntry.GetComponent<NetworkComponent>();
            }
            return _network;
        }
        private set { _network = value; }
    }

    private static ObjectPoolComponent _objectPool;

    /// <summary>  
    /// 获取对象池组件。  
    /// </summary>  
    public static ObjectPoolComponent ObjectPool
    {
        get
        {
            if (_objectPool == null)
            {
                _objectPool = UnityGameFramework.Runtime.GameEntry.GetComponent<ObjectPoolComponent>();
            }
            return _objectPool;
        }
        private set { _objectPool = value; }
    }

    private static ProcedureComponent _procedure;

    /// <summary>  
    /// 获取流程组件。  
    /// </summary>  
    public static ProcedureComponent Procedure
    {
        get
        {
            if (_procedure == null)
            {
                _procedure = UnityGameFramework.Runtime.GameEntry.GetComponent<ProcedureComponent>();
            }
            return _procedure;
        }
        private set { _procedure = value; }
    }

    private static ResourceComponent _resource;

    /// <summary>  
    /// 获取资源组件。  
    /// </summary>  
    public static ResourceComponent Resource
    {
        get
        {
            if (_resource == null)
            {
                _resource = UnityGameFramework.Runtime.GameEntry.GetComponent<ResourceComponent>();
            }
            return _resource;
        }
        private set { _resource = value; }
    }

    private static SceneComponent _scene;

    /// <summary>  
    /// 获取场景组件。  
    /// </summary>  
    public static SceneComponent Scene
    {
        get
        {
            if (_scene == null)
            {
                _scene = UnityGameFramework.Runtime.GameEntry.GetComponent<SceneComponent>();
            }
            return _scene;
        }
        private set { _scene = value; }
    }

    private static SettingComponent _setting;

    /// <summary>  
    /// 获取设置组件。  
    /// </summary>  
    public static SettingComponent Setting
    {
        get
        {
            if (_setting == null)
            {
                _setting = UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
            }
            return _setting;
        }
        private set { _setting = value; }
    }

    private static SoundComponent _sound;

    /// <summary>  
    /// 获取声音组件。  
    /// </summary>  
    public static SoundComponent Sound
    {
        get
        {
            if (_sound == null)
            {
                _sound = UnityGameFramework.Runtime.GameEntry.GetComponent<SoundComponent>();
            }
            return _sound;
        }
        private set { _sound = value; }
    }

    private static UIComponent _ui;

    /// <summary>  
    /// 获取界面组件。  
    /// </summary>  
    public static UIComponent UI
    {
        get
        {
            if (_ui == null)
            {
                _ui = UnityGameFramework.Runtime.GameEntry.GetComponent<UIComponent>();
            }
            return _ui;
        }
        private set { _ui = value; }
    }

    private static WebRequestComponent _webRequest;

    /// <summary>  
    /// 获取网络请求组件。  
    /// </summary>  
    public static WebRequestComponent WebRequest
    {
        get
        {
            if (_webRequest == null)
            {
                _webRequest = UnityGameFramework.Runtime.GameEntry.GetComponent<WebRequestComponent>();
            }
            return _webRequest;
        }
        private set { _webRequest = value; }
    }

    private static void InitBuiltinComponents()
    {
        Base = UnityGameFramework.Runtime.GameEntry.GetComponent<BaseComponent>();
        Config = UnityGameFramework.Runtime.GameEntry.GetComponent<ConfigComponent>();
        DataNode = UnityGameFramework.Runtime.GameEntry.GetComponent<DataNodeComponent>();
        DataTable = UnityGameFramework.Runtime.GameEntry.GetComponent<DataTableComponent>();
        Debugger = UnityGameFramework.Runtime.GameEntry.GetComponent<DebuggerComponent>();
        Download = UnityGameFramework.Runtime.GameEntry.GetComponent<DownloadComponent>();
        Entity = UnityGameFramework.Runtime.GameEntry.GetComponent<EntityComponent>();
        Event = UnityGameFramework.Runtime.GameEntry.GetComponent<EventComponent>();
        FileSystem = UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
        Fsm = UnityGameFramework.Runtime.GameEntry.GetComponent<FsmComponent>();
        Localization = UnityGameFramework.Runtime.GameEntry.GetComponent<LocalizationComponent>();
        Network = UnityGameFramework.Runtime.GameEntry.GetComponent<NetworkComponent>();
        ObjectPool = UnityGameFramework.Runtime.GameEntry.GetComponent<ObjectPoolComponent>();
        Procedure = UnityGameFramework.Runtime.GameEntry.GetComponent<ProcedureComponent>();
        Resource = UnityGameFramework.Runtime.GameEntry.GetComponent<ResourceComponent>();
        Scene = UnityGameFramework.Runtime.GameEntry.GetComponent<SceneComponent>();
        Setting = UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
        Sound = UnityGameFramework.Runtime.GameEntry.GetComponent<SoundComponent>();
        UI = UnityGameFramework.Runtime.GameEntry.GetComponent<UIComponent>();
        WebRequest = UnityGameFramework.Runtime.GameEntry.GetComponent<WebRequestComponent>();
    }
}