using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RoomManager : MonoBehaviour, ISerializationCallbackReceiver
{
    static protected RoomManager s_instance = null;
    static public RoomManager Instance {  get { return s_instance; } }

    public Room[] rooms;
    public RoomCell[] cells;
    public int[] freeRooms;

    protected Dictionary<Vector2Int, RoomCell> _cells;
    protected Vector2 _roomWorldSize;

    void Reset()
    {
        rooms = new Room[0];
        cells = new RoomCell[0];
        freeRooms = new int[0];

        UpdateDictionnary();

        AddRoom(Vector2Int.zero);
    }

    private void Awake()
    {
        s_instance = this;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        UpdateDictionnary();
        ComputeRoomsWorldRect();
    }

    void ComputeRoomsWorldRect()
    {
        Vector2 worldPosition = transform.position;
        _roomWorldSize = new Vector2(RetroScreenSettings.instance.width / (float)RetroScreenSettings.instance.pixelPerUnits, RetroScreenSettings.instance.height / (float)RetroScreenSettings.instance.pixelPerUnits);

        for (int i = 0; i < rooms.Length; ++i)
        {
            if (rooms[i] == null)
                continue;

            RoomCell cell = cells[rooms[i].cell];
            rooms[i].worldRect = new Rect(
                worldPosition.x + cell.coordinate.x * _roomWorldSize.x, 
                worldPosition.y + cell.coordinate.y * _roomWorldSize.y, 
                _roomWorldSize.x * rooms[i].size.x, 
                _roomWorldSize.y * rooms[i].size.y);
        }
    }

    void UpdateDictionnary()
    {
        _cells = new Dictionary<Vector2Int, RoomCell>();

        for (int i = 0; i < cells.Length; ++i)
        {
            _cells[cells[i].coordinate] = cells[i];
        }
    }

    public RoomCell GetCellFromWorld(Vector2 world)
    {
        int coordX = Mathf.FloorToInt(world.x / _roomWorldSize.x);
        int coordY = Mathf.FloorToInt(world.y / _roomWorldSize.y);

        RoomCell cell = null;
        _cells.TryGetValue(new Vector2Int(coordX, coordY), out cell);

        return cell;
    }

    public Room GetRoom(Vector2Int coordinate)
    {
        RoomCell c;
        if (_cells.TryGetValue(coordinate, out c) && c.room != -1)
            return rooms[c.room];

        return null;
    }

    public RoomCell GetRoomCell(Vector2Int coordinate)
    {
        RoomCell c;
        if (_cells.TryGetValue(coordinate, out c))
            return c;

        return null;
    }

#if UNITY_EDITOR

    RoomCell CreateCell(Vector2Int coord)
    {
        RoomCell cell = new RoomCell(coord, cells.Length);
        UnityEditor.ArrayUtility.Add(ref cells, cell);

        _cells[coord] = cell;

        return cell;
    }

    public void AddRoom(Vector2Int coord)
    {
        RoomCell cell;
        if(!_cells.TryGetValue(coord, out cell))
        {
            cell = CreateCell(coord);
        }

        int index = 0;
        if (freeRooms.Length > 0)
        {
            index = freeRooms[0];
            UnityEditor.ArrayUtility.RemoveAt(ref freeRooms, 0);
        }
        else
        {
            index = rooms.Length;
            UnityEditor.ArrayUtility.Add(ref rooms, null);
        }

        Room newRoom = new Room(cell.arrayIdx, index);
        rooms[index] = newRoom;

        cell.room = newRoom.arrayIdx;

        SetupCellSurrounding(cell);
    }

    void SetupCellSurrounding(RoomCell cell)
    {
        RoomCell tempCell;
        if (_cells.TryGetValue(cell.coordinate + Vector2Int.left, out tempCell))
        {
            tempCell.right = cell.room;
            cell.left = tempCell.room;
        }

        if (_cells.TryGetValue(cell.coordinate + Vector2Int.right, out tempCell))
        {
            tempCell.left = cell.room;
            cell.right = tempCell.room;
        }

        if (_cells.TryGetValue(cell.coordinate + Vector2Int.up, out tempCell))
        {
            tempCell.down = cell.room;
            cell.top = tempCell.room;
        }

        if (_cells.TryGetValue(cell.coordinate + Vector2Int.down, out tempCell))
        {
            tempCell.top = cell.room;
            cell.down = tempCell.room;
        }
    }

    public void ExpandRoom(Room r, Vector2Int direction)
    {
        if(direction.x == 1)
        {
            for(int y = 0; y < r.size.y; ++y)
            {
                RoomCell currentCell = GetRoomCell(cells[r.cell].coordinate + Vector2Int.right * (r.size.x - 1) + Vector2Int.up * y);
                Vector2Int targetCellCoord = currentCell.coordinate + Vector2Int.right;

                RoomCell targetCell;
                if (!_cells.TryGetValue(targetCellCoord, out targetCell))
                {
                    targetCell = CreateCell(targetCellCoord);
                }

                targetCell.room = r.arrayIdx;

                SetupCellSurrounding(targetCell);
            }
        }
        else if(direction.x == -1)
        {
            //in the case of expanding to the left, we need to change the origin cell, as it should always be the bottom left one
            int newOriginCell = r.cell;
            for (int y = 0; y < r.size.y; ++y)
            {
                RoomCell currentCell = GetRoomCell(cells[r.cell].coordinate + Vector2Int.up * y);
                Vector2Int targetCellCoord = currentCell.coordinate + Vector2Int.left;

                RoomCell targetCell;
                if (!_cells.TryGetValue(targetCellCoord, out targetCell))
                {
                    targetCell = CreateCell(targetCellCoord);
                }

                if (y == 0)
                    newOriginCell = targetCell.arrayIdx;

                targetCell.room = r.arrayIdx;

                SetupCellSurrounding(targetCell);
            }

            r.cell = newOriginCell;
        }

        else if (direction.y == 1)
        {
            for (int x = 0; x < r.size.x; ++x)
            {
                RoomCell currentCell = GetRoomCell(cells[r.cell].coordinate + Vector2Int.up * (r.size.y - 1) + Vector2Int.right * x);
                Vector2Int targetCellCoord = currentCell.coordinate + Vector2Int.up;

                RoomCell targetCell;
                if (!_cells.TryGetValue(targetCellCoord, out targetCell))
                {
                    targetCell = CreateCell(targetCellCoord);
                }

                targetCell.room = r.arrayIdx;

                SetupCellSurrounding(targetCell);
            }
        }
        else if (direction.y == -1)
        {
            //in the case of expanding to the bottom, we need to change the origin cell, as it should always be the bottom left one
            int newOriginCell = r.cell;
            for (int x = 0; x < r.size.x; ++x)
            {
                RoomCell currentCell = GetRoomCell(cells[r.cell].coordinate + Vector2Int.right * x);
                Vector2Int targetCellCoord = currentCell.coordinate + Vector2Int.down;

                RoomCell targetCell;
                if (!_cells.TryGetValue(targetCellCoord, out targetCell))
                {
                    targetCell = CreateCell(targetCellCoord);
                }

                if (x == 0)
                    newOriginCell = targetCell.arrayIdx;

                targetCell.room = r.arrayIdx;

                SetupCellSurrounding(targetCell);
            }

            r.cell = newOriginCell;
        }

        r.size += new Vector2Int(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
    }

    public void RemoveRoom(Room r)
    {
        Vector2Int originalCoord = cells[r.cell].coordinate;
        for(int y = 0; y < r.size.y; ++y)
        {
            for(int x = 0; x < r.size.x; ++x)
            {
                Vector2Int coord = originalCoord + new Vector2Int(x, y);

                RoomCell c = _cells[coord];
                c.room = -1;

                RoomCell leftcell = GetRoomCell(coord + Vector2Int.left);
                if (leftcell != null)
                    leftcell.right = -1;

                RoomCell rightcell = GetRoomCell(coord + Vector2Int.right);
                if (rightcell != null)
                    rightcell.left = -1;

                RoomCell topcell = GetRoomCell(coord + Vector2Int.up);
                if (topcell != null)
                    topcell.down = -1;

                RoomCell bottomcell = GetRoomCell(coord + Vector2Int.down);
                if (bottomcell != null)
                    bottomcell.top = -1;
            }
        }

        int idx = r.arrayIdx;
        rooms[idx] = null;
        UnityEditor.ArrayUtility.Add(ref freeRooms, idx);
    }
#endif

    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        UpdateDictionnary();
    }
}

[System.Serializable]
public class Room
{
    public int arrayIdx;
    public int cell;
    public Vector2Int size;

    public Rect worldRect;

    public Room(int cell, int index)
    {
        this.cell = cell;
        arrayIdx = index;
        size = new Vector2Int(1, 1);
    }
}

[System.Serializable]
public class RoomCell
{
    public Vector2Int coordinate;
    public int arrayIdx;
    public int room;
    public int top, right, down, left;

    public RoomCell(Vector2Int coord, int index)
    {
        coordinate = coord;
        top = right = down = left = -1;
        room = -1;
        arrayIdx = index;
    }
}