using System;
using GameFramework;
using UnityEngine;

public static class AssetUtility
{
    public static string GetReadWriteVideoAsset(string assetName)
    {
        return Utility.Text.Format("{0}/{1}",
            Application.platform == RuntimePlatform.WindowsEditor ? Application.streamingAssetsPath : Application.persistentDataPath,
            assetName);
    }

    public static string GetConfigAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/Configs/{0}.{1}", assetName, "txt");
    }

    public static string GetDataTableAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/DataTables/{0}.{1}", assetName, "txt");
    }

    public static string GetDictionaryAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/Localization/{0}/Dictionaries/{1}.{2}",
            GameEntry.Localization.Language.ToString(), assetName, "xml");
    }

    public static string GetFontAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/Fonts/{0}.ttf", assetName);
    }

    public static string GetTextMeshProFontAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/Fonts/{0}.asset", assetName);
    }

    public static string GetSceneAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/Scenes/{0}.unity", assetName);
    }

    public static string GetMusicAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/Music/{0}.mp3", assetName);
    }

    public static string GetSoundAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/Sounds/{0}.wav", assetName);
    }

    public static string GetEntityAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/Entities/{0}.prefab", assetName);
    }

    public static string GetUIFormAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/UI/UIForms/{0}.prefab", assetName);
    }

    public static string GetUISoundAsset(string assetName)
    {
        return Utility.Text.Format("Assets/GameMain/Resources/UI/UISounds/{0}.wav", assetName);
    }
}
