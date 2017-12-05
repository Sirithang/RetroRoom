using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[CreateAssetMenu(fileName = "InputMapping", menuName = "Input/Mapping")]
public class InputMapping : ScriptableObject
{
    //Bump every time serialized data alyout change. Allow to handle compat with save file created before the change
    private static int VERSION = 1;
    
    [System.Serializable]
    public class InputButton
    {
        public string name;
        public KeyCode key;
    }

    public InputButton[] buttons;

    [HideInInspector] public string UUID = System.Guid.NewGuid().ToString();
    
    protected Dictionary<int, KeyCode> _internalMapping;
    protected string _saveFileName;
    
    private void OnEnable()
    {
        _saveFileName = UUID + "_input.dat";
        _internalMapping = new Dictionary<int, KeyCode>();

        if (buttons != null)
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                int hash = NameToHash(buttons[i].name);
                _internalMapping[hash] = buttons[i].key;
            }
        }
    }

    //as the change will be made at runtime, the data changed in the button array can't be serilized by Unity
    //so we manually save to a file. The Unity asset file is only useful in editor as "default" mapping
    public void Save()
    {
        string savePosition = DataManager.GetCurrentUserSavePosition();
        
        if(savePosition == null)
            return;
        
        BinaryWriter writer = new BinaryWriter(new FileStream(savePosition+"/"+_saveFileName, FileMode.Create));

        writer.Write(VERSION);
        writer.Write(_internalMapping.Count);
        foreach(var pair in _internalMapping)
        {
            writer.Write(pair.Key);
            writer.Write((int)pair.Value);
        }
    }

    public void Load()
    {
        string savePosition = DataManager.GetCurrentUserSavePosition();
        
        if(savePosition == null || !File.Exists(savePosition+"/"+_saveFileName))
            return;
        
        BinaryReader reader = new BinaryReader(new FileStream(savePosition+"/"+_saveFileName, FileMode.Open));

        int ver = reader.ReadInt32();
        int count = reader.ReadInt32();
        
        for (int i = 0; i < count; ++i)
        {
            //this is the name already 
            int name = reader.ReadInt32();
            KeyCode code = (KeyCode) reader.ReadInt32();

            _internalMapping[name] = code;
        }
    }

    public void ChangeKeycode(int hash, KeyCode code)
    {
        _internalMapping[hash] = code;
    }

    public void ChangeKeycode(string keyName, KeyCode code)
    {
        ChangeKeycode(NameToHash(keyName), code);
    }

    public KeyCode GetKeyCode(int hash)
    {
        return _internalMapping[hash];
    }

    public KeyCode GetKeyCode(string keyName)
    {
        return GetKeyCode(NameToHash(keyName));
    }

    static public int NameToHash(string name)
    {
        //for now use the animator one, avoid redoing a hash function
        return Animator.StringToHash(name);
    }
}
