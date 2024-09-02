using GameFramework.Event;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityGameFramework.Runtime;

public class GameManager : GameFrameworkComponent
{
    public uint guid;

    public int RockerId;

    private void Start()
    {
        //订阅
        GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccessEventArgs);
    }

    public void Init()
    {
        NetManager.AddMsgListener("MsgGetInfo", OnMsgGetInfo);
        NetManager.AddMsgListener("MsgStart", OnMsgStart);
        NetManager.AddMsgListener("MsgMoveBack", OnMsgMoveBack);
        NetManager.AddMsgListener("MsgStop", OnMsgStop);
        NetManager.SendTo(new MsgGetInfo(),NetManager.ServerType.Fighter);

        GameEntry.Setting.SetBool("isZTB",true);
        GameEntry.Setting.Save();
    }

    private void OnMsgStop(MsgBase msgBase)
    {
        if (Roles.Instance.mRoles.ContainsKey(guid))
        {
            Role role = Roles.Instance.mRoles[guid];
            Roles.Instance.mRoles.Remove(guid);
            GameEntry.Entity.HideEntity(role.m_entityId);
        }
    }

    private void OnMsgMoveBack(MsgBase msgBase)
    {
        MsgMoveBack msgMoveBack = (MsgMoveBack)msgBase;
        uint MyGuid = msgMoveBack.opts.guid;

        Role role = Roles.Instance.mRoles[MyGuid];
        role.OnOpts(msgMoveBack.opts);
    }

    private void OnMsgGetInfo(MsgBase msgBase)
    {
        MsgGetInfo msg = (MsgGetInfo)msgBase;
        guid = msg.guid;

        MsgStart msgStart = new MsgStart();
        NetManager.SendTo(msgStart, NetManager.ServerType.Fighter);

        GameEntry.Entity.ShowEntity<Role>(0, AssetUtility.GetEntityAsset("Roles/Role"), "Role"
                , new object[]
                    {
                        this, guid , 0
                    });

        GameEntry.Setting.SetBool("isZTB", false);
        GameEntry.Setting.Save();
    }

    private void OnShowEntitySuccessEventArgs(object sender, GameEventArgs e)
    {
        ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
        object[] objects = ne.UserData as object[];

        if (objects != null && objects[0] == this)
        {
            EntityLogic logic = ne.Entity.Logic;
            Role role = logic as Role;
            if (role != null)
            {
                role.guid = (uint)objects[1];
                role.m_entityId = (int)objects[2];
                Roles.Instance.Add(role.guid, role);
            }
        }
    }

    private void OnMsgStart(MsgBase msgBase)
    {
        MsgStart msgStart = (MsgStart)msgBase;

        if (!msgStart.res)
        {
            return;
        }

        for (int i = 0; i < msgStart.guid.Length; i++)
        {
            if (GameEntry.GameManager.guid != msgStart.guid[i])
            {
                GameEntry.Entity.ShowEntity<Role>(i + 1, AssetUtility.GetEntityAsset("Roles/Role"), "Role"
                , new object[]
                    {
                        this, msgStart.guid[i],i + 1
                    }
            );
            }

        }

        StartCoroutine(WaitForRolesAndExecute(msgStart));
    }

    private IEnumerator WaitForRolesAndExecute(MsgStart msgStart)
    {
        while (Roles.Instance.mRoles.Count < msgStart.guid.Length)
        {
            // 等待一帧  
            yield return null;
        }

        if (Roles.Instance.mRoles.ContainsKey(guid))
        {
            Role role = Roles.Instance.mRoles[guid];
            Vector2 position = role.gameObject.transform.position;

            Opts opts = new Opts();
            Fixed64[] fixed64 = new Fixed64[2] {
                (Fixed64)position.x,
                (Fixed64)position.y,
            };
            opts.param = fixed64;
            opts.operation = Operation.MoveLocation;
            opts.guid = guid;

            MsgMove msgMove = new MsgMove();
            msgMove.opts = opts;

            NetManager.SendTo(msgMove, NetManager.ServerType.Fighter);
        }
        GameEntry.Setting.SetBool("isZTB", true);
        GameEntry.Setting.Save();

        Debug.Log("开启帧同步");
    }
}