using Cinemachine;
using GameFramework.Entity;
using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;
using ShowEntitySuccessEventArgs = UnityGameFramework.Runtime.ShowEntitySuccessEventArgs;

public class Roles : Singleton<Roles>
{
    
    public Dictionary<uint, Role> mRoles = new Dictionary<uint, Role>();


    private Vector2 movement;

    public Role GetRole(uint guid)
    {
        if (mRoles.ContainsKey(guid))
        {
            return mRoles[guid];
        }
        return null;
    }

    public void Add(uint guid,Role role)
    {
        mRoles.Add(guid, role);
    }

    protected  void OnInit(object userData)
    {
        //订阅
        GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, showEntitySuccessEventArgs);

        
    }

    private void showEntitySuccessEventArgs(object sender, GameEventArgs e)
    {
        ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
        if (ne.UserData != this)
        {
            return;
        }
        //摄像头设置
        EntityLogic entityLogic = ne.Entity.Logic;
        Role role = (Role)entityLogic;
        GameObject Virtua1Camera = GameObject.Find("Virtual Camera");
        CinemachineVirtualCamera cinemachineVirtualCamera = Virtua1Camera.GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = entityLogic.transform;
        GameEntry.Entity.AttachEntity(mRoles.Count + 1,0);
        
    }

    protected  void OnShow(object userData)
    {
        GameEntry.Entity.ShowEntity<Role>(mRoles.Count+1, AssetUtility.GetEntityAsset("Roles/Role"), "Role", this);

        NetManager.Send(new MsgGetInfo(),NetManager.ServerType.Fighter);
    }

    protected  void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        // 获取输入
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        Vector2 movementThisFrame = movement * Time.fixedDeltaTime;
        
    }


}