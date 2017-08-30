using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerEditor : Editor
{
    RoomManager _manager;

    void OnEnable()
    {
        _manager = target as RoomManager;
    }


    void OnSceneGUI()
    {
        Vector3 screenSize = new Vector3(RetroScreenSettings.instance.width / RetroScreenSettings.instance.pixelPerUnits, RetroScreenSettings.instance.height / RetroScreenSettings.instance.pixelPerUnits, 0);

        for (int i = 0; i < _manager.rooms.Length; ++i)
        {
            Room room = _manager.rooms[i];
            RoomCell originCell = _manager.cells[room.cell];

            Rect r = new Rect(originCell.coordinate.x * screenSize.x, originCell.coordinate.y * screenSize.y, screenSize.x * room.size.x, screenSize.y * room.size.y);

            Handles.color = Color.red;
            Handles.DrawSolidRectangleWithOutline(r, new Color(0, 0, 0, 0), Color.red);

            Handles.color = Color.green;
            for(int y = 0; y < room.size.y; ++y)
            {
                for(int x = 0; x < room.size.x; ++x)
                {
                    RoomCell cell = _manager.GetRoomCell(originCell.coordinate + new Vector2Int(x, y));

                    Vector3 origin = (Vector3)r.min + Vector3.right * screenSize.x * x + Vector3.up * screenSize.y * y;

                    //left add
                    if (cell.left == -1)
                    {
                        if (Handles.Button(origin + Vector3.up * screenSize.y * 0.5f, Quaternion.LookRotation(Vector3.forward), 0.2f, 0.2f, Handles.RectangleHandleCap))
                        {
                            _manager.AddRoom(new Vector2Int(cell.coordinate.x - 1, cell.coordinate.y));
                        }
                    }

                    //down add
                    if (cell.down == -1)
                    {
                        if (Handles.Button(origin + Vector3.right * screenSize.x * 0.5f, Quaternion.LookRotation(Vector3.forward), 0.2f, 0.2f, Handles.RectangleHandleCap))
                        {
                            _manager.AddRoom(new Vector2Int(cell.coordinate.x, cell.coordinate.y - 1));
                        }
                    }

                    //right add
                    if (cell.right == -1)
                    {
                        if (Handles.Button(origin + screenSize + Vector3.down * screenSize.y * 0.5f, Quaternion.LookRotation(Vector3.forward), 0.2f, 0.2f, Handles.RectangleHandleCap))
                        {
                            _manager.AddRoom(new Vector2Int(cell.coordinate.x + room.size.x, cell.coordinate.y));
                        }
                    }

                    //top add
                    if (cell.top == -1)
                    {
                        if (Handles.Button(origin + screenSize + Vector3.left * screenSize.x * 0.5f, Quaternion.LookRotation(Vector3.forward), 0.2f, 0.2f, Handles.RectangleHandleCap))
                        {
                            _manager.AddRoom(new Vector2Int(cell.coordinate.x, cell.coordinate.y + room.size.y));
                        }
                    }
                }
            }


            

            //room extension

            Handles.color = Color.blue;

            if (Handles.Button((Vector3)r.center + Vector3.right * r.width * 0.2f, Quaternion.LookRotation(Vector3.right), 0.4f, 0.2f, Handles.ConeHandleCap))
            { // right

            }

            if (Handles.Button((Vector3)r.center + Vector3.down * r.height * 0.2f, Quaternion.LookRotation(Vector3.down), 0.4f, 0.2f, Handles.ConeHandleCap))
            { // down

            }

            if (Handles.Button((Vector3)r.center + Vector3.left * r.width * 0.2f, Quaternion.LookRotation(Vector3.left), 0.4f, 0.2f, Handles.ConeHandleCap))
            { // left

            }

            if (Handles.Button((Vector3)r.center + Vector3.up * r.height * 0.2f, Quaternion.LookRotation(Vector3.up), 0.4f, 0.2f, Handles.ConeHandleCap))
            { // up

            }
        }
    }
}
