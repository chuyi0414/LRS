using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class Setting : UIFormLogic
{
    public Button BtnSetting;
    protected override void OnOpen(object userData)
    {
        BtnSetting.onClick.AddListener(OnBtnSetting);
    }

    private void OnBtnSetting()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
