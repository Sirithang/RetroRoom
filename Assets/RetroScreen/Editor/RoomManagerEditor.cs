using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerEditor : Editor
{
    RoomManager _manager;

    static bool s_DisplayRooms = true;

    static RoomManagerEditor()
    {
        SceneView.onSceneGUIDelegate += StaticSceneGUI;
    }

    static void StaticSceneGUI(SceneView view)
    {

        Handles.BeginGUI();
        GUI.Box(new Rect(0, Screen.height - 70, 200, 70), "Room Display Setting", EditorStyles.textArea);
        s_DisplayRooms = GUI.Toggle(new Rect(10, Screen.height - 55, 180, 25), s_DisplayRooms, "Display rooms");
        Handles.EndGUI();

        RoomManager[] managers = Resources.FindObjectsOfTypeAll<RoomManager>();

        if(managers.Length > 1)
        {
            Handles.BeginGUI();
            GUI.Label(new Rect(0, 0, 300, 50), "[ERROR] : You have more than 1 room manager in your scene.");
            Handles.EndGUI();
        }
        
        if(managers.Length == 1 && s_DisplayRooms)
        {
            SceneGUIGeneric(false, managers[0]);
        }
    }

    void OnEnable()
    {
        _manager = target as RoomManager;
    }

    static void SceneGUIGeneric(bool displayControls, RoomManager manager)
    {
        Vector3 screenSize = new Vector3(RetroScreenSettings.instance.width / (float)RetroScreenSettings.instance.pixelPerUnits, RetroScreenSettings.instance.height / (float)RetroScreenSettings.instance.pixelPerUnits, 0);

        int removeRoom = -1;
        for (int i = 0; i < manager.rooms.Length; ++i)
        {
            Room room = manager.rooms[i];
            if (room == null)
                continue;

            RoomCell originCell = manager.cells[room.cell];

            Rect r = new Rect(originCell.coordinate.x * screenSize.x, originCell.coordinate.y * screenSize.y, screenSize.x * room.size.x, screenSize.y * room.size.y);

            Handles.color = Color.red;
            Handles.DrawSolidRectangleWithOutline(r, new Color(0, 0, 0, 0), Color.red);

            Handles.color = Color.white;
            Handles.Label(r.center, i.ToString(), EditorStyles.whiteLargeLabel);

            if (!displayControls)
                continue;

            Handles.color = Color.red;
            if(Handles.Button(r.center, Quaternion.LookRotation(Vector3.forward), 0.2f, 0.2f, Handles.CircleHandleCap))
            {
                removeRoom = i;
            }

            bool leftExpand = true;
            bool rightExpand = true;
            bool upExpand = true;
            bool downExpand = true; 

            Handles.color = Color.green;
            for (int y = 0; y < room.size.y; ++y)
            {
                for (int x = 0; x < room.size.x; ++x)
                {
                    RoomCell cell = manager.GetRoomCell(originCell.coordinate + new Vector2Int(x, y));

                    Vector3 origin = (Vector3)r.min + Vector3.right * screenSize.x * x + Vector3.up * screenSize.y * y;

                    //left add
                    if (cell.left == -1)
                    {
                        if (Handles.Button(origin + Vector3.up * screenSize.y * 0.5f + Vector3.left * screenSize.x * 0.5f, Quaternion.LookRotation(Vector3.forward), 0.2f, 0.2f, Handles.RectangleHandleCap))
                        {
                            manager.AddRoom(new Vector2Int(cell.coordinate.x - 1, cell.coordinate.y));
                        }
                    }
                    else if(cell.left != room.arrayIdx)
                        leftExpand = false;

                    //down add
                    if (cell.down == -1)
                    {
                        if (Handles.Button(origin + Vector3.right * screenSize.x * 0.5f + Vector3.down * screenSize.y * 0.5f, Quaternion.LookRotation(Vector3.forward), 0.2f, 0.2f, Handles.RectangleHandleCap))
                        {
                            manager.AddRoom(new Vector2Int(cell.coordinate.x, cell.coordinate.y - 1));
                        }
                    }
                    else if(cell.down != room.arrayIdx)
                        downExpand = false;

                    //right add
                    if (cell.right == -1)
                    {
                        if (Handles.Button(origin + screenSize + Vector3.down * screenSize.y * 0.5f + Vector3.right * screenSize.x * 0.5f, Quaternion.LookRotation(Vector3.forward), 0.2f, 0.2f, Handles.RectangleHandleCap))
                        {
                            manager.AddRoom(new Vector2Int(cell.coordinate.x + room.size.x, cell.coordinate.y));
                        }
                    }
                    else if (cell.right != room.arrayIdx)
                        rightExpand = false;

                    //top add
                    if (cell.top == -1)
                    {
                        if (Handles.Button(origin + screenSize + Vector3.left * screenSize.x * 0.5f + Vector3.up * screenSize.y * 0.5f, Quaternion.LookRotation(Vector3.forward), 0.2f, 0.2f, Handles.RectangleHandleCap))
                        {
                            manager.AddRoom(new Vector2Int(cell.coordinate.x, cell.coordinate.y + room.size.y));
                        }
                    }
                    else if (cell.top != room.arrayIdx)
                        upExpand = false;
                }
            }




            //room extension
            
            Handles.color = Color.blue;

            if (rightExpand && Handles.Button((Vector3)r.center + Vector3.right * r.width * 0.4f, Quaternion.LookRotation(Vector3.right), 0.4f, 0.2f, Handles.ConeHandleCap))
            { // right
                manager.ExpandRoom(room, Vector2Int.right);
            }

            if (downExpand && Handles.Button((Vector3)r.center + Vector3.down * r.height * 0.4f, Quaternion.LookRotation(Vector3.down), 0.4f, 0.2f, Handles.ConeHandleCap))
            { // down
                manager.ExpandRoom(room, Vector2Int.down);
            }

            if (leftExpand && Handles.Button((Vector3)r.center + Vector3.left * r.width * 0.4f, Quaternion.LookRotation(Vector3.left), 0.4f, 0.2f, Handles.ConeHandleCap))
            { // left
                manager.ExpandRoom(room, Vector2Int.left);
            }

            if (upExpand && Handles.Button((Vector3)r.center + Vector3.up * r.height * 0.4f, Quaternion.LookRotation(Vector3.up), 0.4f, 0.2f, Handles.ConeHandleCap))
            { // up
                manager.ExpandRoom(room, Vector2Int.up);
            }
        }


        if(removeRoom != -1)
        {
            manager.RemoveRoom(manager.rooms[removeRoom]);
        }
    }

    void OnSceneGUI()
    {
        SceneGUIGeneric(true, _manager);
    }
}
