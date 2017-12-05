using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

//Useful class to handle data persistance. Using GetUserFolder() return the folder where to save data for the current
//user. If no user name are set, saving will fail (so if the game don't handle multiple session, just set the username
//to something like default. In Editor, if no user exist, to help dev, a default "DevDefault" is created.
public class DataManager
{
    static DataManager _instance;
    
    List<string> _usersList = new List<string>();
    string _currentUser = "";

    private string _savePlace;

    [RuntimeInitializeOnLoadMethod]
    static void RuntimeInit()
    {
        _instance = new DataManager();
        
        _instance._savePlace = Application.persistentDataPath + "/SavedData";
        _instance.LoadUsersList();
    }

    void LoadUsersList()
    {
        
        if (!Directory.Exists(_savePlace))
            Directory.CreateDirectory(_savePlace);
        
        var dirs = Directory.GetDirectories(_savePlace);

        if (dirs.Length == 0)
        {
#if UNITY_EDITOR
            Directory.CreateDirectory(_savePlace+"/DevDefault");
            _currentUser = "DevDefault";
            _usersList.Add(_currentUser);
#endif
        }
        else
        {
            foreach (var dir in dirs)
            {
                var properDir = dir.Replace("\\", "/");
                string userName = properDir.Replace(_savePlace + "/", "");
                 
                _usersList.Add(userName);
            }
            
#if UNITY_EDITOR
            //in editor we set DevDefault as the default user
            _currentUser = "DevDefault";
#endif
        }
    }
    
    
    public static List<string> GetUsersList()
    {
        return _instance._usersList;
    }

    //CAREFUL, this is definitive, be sure to display a prompt for your users before calling that function!!
    public static void RemoveUser(string name)
    {
        if (!_instance._usersList.Contains(name))
        {
            Debug.LogError("Users don't exist : " + name);
            return;
        }
        
        Directory.Delete(_instance._savePlace+"/"+name);
        _instance._usersList.Remove(name);
    }

    public static void SetCurrentUser(string currentUser)
    {
        if (!_instance._usersList.Contains(currentUser))
        {
            Debug.LogError("Users don't exist : " + currentUser);
            return;
        }
        
        _instance._currentUser = currentUser;
    }

    public static string GetCurrentUserSavePosition()
    {
        return GetUserSavePosition(_instance._currentUser);
    }

    public static string GetUserSavePosition(string user)
    {
        if (!_instance._usersList.Contains(user))
        {
            Debug.LogError("Users don't exist : " + user);
            return null;
        }

        return _instance._savePlace+"/"+user;
    }
}
