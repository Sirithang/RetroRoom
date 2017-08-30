using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "retrosettings", menuName ="retrosetting")]
public class RetroScreenSettings : ScriptableObject
{
    static public RetroScreenSettings instance { get { if (s_instance == null) LoadInstance(); return s_instance; } }
    static protected RetroScreenSettings s_instance = null;

    public int width = 256;
    public int height = 224;
    public int pixelPerUnits = 32;

    static RetroScreenSettings() 
    {
#if UNITY_EDITOR
        EditorApplication.update += EdUpdate;
#endif
    }

    static void LoadInstance()
    {
        RetroScreenSettings setting = Resources.Load<RetroScreenSettings>("retroscreensettings");

        if (setting != null)
            s_instance = setting;
    }

#if UNITY_EDITOR

    static public void EdUpdate()
    {
        LoadInstance();

        if (s_instance == null)
        {
            if (EditorUtility.DisplayDialog("Retro Screen settings", "The system can't find a retro screen settings in the root of the Resources folder, create one?", "Yes", "no"))
            {
                s_instance = ScriptableObject.CreateInstance<RetroScreenSettings>();

                if(!System.IO.Directory.Exists(Application.dataPath + "/Resources"))
                {
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources");
                }

                AssetDatabase.CreateAsset(s_instance, "Assets/Resources/retroscreensettings.asset");
                AssetDatabase.Refresh();
            }
        }

        EditorApplication.update -= EdUpdate;
    }

#endif
}