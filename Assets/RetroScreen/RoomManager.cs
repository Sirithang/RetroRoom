using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RoomManager : MonoBehaviour, ISerializationCallbackReceiver
{
    public Room[] rooms;
    public RoomCell[] cells;
    public int[] freeRooms;

    protected Dictionary<Vector2Int, RoomCell> _cells;

    void Reset()
    {
        rooms = new Room[0];

        AddRoom(Vector2Int.zero);

        freeRooms = new int[0];
    }

    void UpdateDictionnary()
    {
        _cells = new Dictionary<Vector2Int, RoomCell>();

        for (int i = 0; i < cells.Length; ++i)
        {
            _cells[cells[i].coordinate] = cells[i];
        }
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
    public void AddRoom(Vector2Int coord)
    {
        RoomCell cell;
        if(!_cells.TryGetValue(coord, out cell))
        {
            cell = new RoomCell(coord, cells.Length);
            UnityEditor.ArrayUtility.Add(ref cells, cell);

            _cells[coord] = cell;
        }

        Room newRoom = new Room(cell.arrayIdx, rooms.Length);
        UnityEditor.ArrayUtility.Add(ref rooms, newRoom);

        cell.room = newRoom.arrayIdx;

        RoomCell tempCell;
        if(_cells.TryGetValue(coord + Vector2Int.left, out tempCell))
        {
            tempCell.right = cell.room;
            cell.left = tempCell.room;
        }

        if (_cells.TryGetValue(coord + Vector2Int.right, out tempCell))
        {
            tempCell.left = cell.room;
            cell.right = tempCell.room;
        }

        if (_cells.TryGetValue(coord + Vector2Int.up, out tempCell))
        {
            tempCell.down = cell.room;
            cell.top = tempCell.room;
        }

        if (_cells.TryGetValue(coord + Vector2Int.down, out tempCell))
        {
            tempCell.top = cell.room;
            cell.down = tempCell.room;
        }
    }

    public void ExpandRoom(Room r, Vector2Int direction)
    {

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