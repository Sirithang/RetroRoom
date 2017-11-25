using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    static LevelSettings s_Instance;
    static public LevelSettings Instance
    {
        get
        {
#if UNITY_EDITOR
            //we only tets that in editor, build game is suppose to have one in all level, testing every time we try
            //to access the setting instance will add useless computation.
            if(s_Instance == null)
            {
                Debug.LogError("Your level don't contains a LevelSetting!");
            }
#endif
            return s_Instance;
        }
    }

    public float gravity;

    private void Awake()
    {
        s_Instance = this;
    }
}
