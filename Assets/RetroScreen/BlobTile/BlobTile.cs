using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlobTile : Tile
{
    [System.Serializable]
    public class PlatformerTileEntry
    {
        public Sprite[] sprite = new Sprite[4];
        public float[] rotation = new float[4];
    }

    public PlatformerTileEntry[] entries = new PlatformerTileEntry[15];
    public bool missingReturnFull = true;

    public BlobTile() : base()
    {
        for(int i = 0; i < entries.Length; ++i)
        {
            entries[i] = new PlatformerTileEntry();
        }
    }

#if UNITY_EDITOR
    
    [MenuItem("Assets/Create/Tile/Blob Tile")]
    static public void MenuEntryCreate()
    {
        CreateFile();
    }

    static public void CreateFile(string path = "", string name = "")
    {
        if(path == "")
            path = EditorUtility.SaveFilePanelInProject("Save Terrain Tile", "New Terrain Tile", "asset", "Save Terrain Tile", "Assets");
        else
        {
            if (name == "")
                name = "NewBlobTile.asset";

            path = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name);
        }

        if (path == "")
            return;

        AssetDatabase.CreateAsset(CreateInstance<BlobTile>(), path);
    }
#endif

    public override void  GetTileData(Vector3Int position, ITilemap Tilemap, ref TileData tileData)
    {
        UpdateTile(position, Tilemap, ref tileData);
    }

    public override void RefreshTile(Vector3Int location, ITilemap Tilemap)
    {
        for (int yd = -1; yd <= 1; yd++)
        {
            for (int xd = -1; xd <= 1; xd++)
            {
                Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                if (TileValue(Tilemap, position))
                    Tilemap.RefreshTile(position);
            }
        }
    }

    private bool TileValue(ITilemap Tilemap, Vector3Int position)
    {
        TileBase tile = Tilemap.GetTile(position);
        return (tile != null && (tile.GetType() == this.GetType()));
    }


    private void UpdateTile(Vector3Int location, ITilemap Tilemap, ref TileData tileData)
    {
        tileData.transform = Matrix4x4.identity;
        tileData.color = Color.white;

        int mask = TileValue(Tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
        mask += TileValue(Tilemap, location + new Vector3Int(1, 1, 0)) ? 2 : 0;
        mask += TileValue(Tilemap, location + new Vector3Int(1, 0, 0)) ? 4 : 0;
        mask += TileValue(Tilemap, location + new Vector3Int(1, -1, 0)) ? 8 : 0;
        mask += TileValue(Tilemap, location + new Vector3Int(0, -1, 0)) ? 16 : 0;
        mask += TileValue(Tilemap, location + new Vector3Int(-1, -1, 0)) ? 32 : 0;
        mask += TileValue(Tilemap, location + new Vector3Int(-1, 0, 0)) ? 64 : 0;
        mask += TileValue(Tilemap, location + new Vector3Int(-1, 1, 0)) ? 128 : 0;

        byte original = (byte)mask;
        if ((original | 254) < 255) { mask = mask & 125; }
        if ((original | 251) < 255) { mask = mask & 245; }
        if ((original | 239) < 255) { mask = mask & 215; }
        if ((original | 191) < 255) { mask = mask & 95; }


        int entry = MaskToEntryIndex((byte)mask);
        int spr = MaskToInternalIndex((byte)mask);

        Sprite s = entries[entry].sprite[spr];

        if (s == null && missingReturnFull)
            s = entries[14].sprite[0];

        if (TileValue(Tilemap, location) && s != null)
        {
            tileData.color = Color.white;
            tileData.flags = (TileFlags.LockTransform | TileFlags.LockColor);
            tileData.colliderType = Tile.ColliderType.Sprite;
            tileData.sprite = s;
            tileData.transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, -entries[entry].rotation[spr]), Vector3.one);
        }
    }


    static public int MaskToEntryIndex(byte mask)
    {
        switch (mask)
        {
            case 0: return 0;
            case 1:
            case 4:
            case 16:
            case 64: return 1;
            case 5:
            case 20:
            case 80:
            case 65: return 2;
            case 7:
            case 28:
            case 112:
            case 193: return 3;
            case 17:
            case 68: return 4;
            case 21:
            case 84:
            case 81:
            case 69: return 5;
            case 23:
            case 92:
            case 113:
            case 197: return 6;
            case 29:
            case 116:
            case 209:
            case 71: return 7;
            case 31:
            case 124:
            case 241:
            case 199: return 8;
            case 85: return 9;
            case 87:
            case 93:
            case 117:
            case 213: return 10;
            case 95:
            case 125:
            case 245:
            case 215: return 11;
            case 119:
            case 221: return 12;
            case 127:
            case 253:
            case 247:
            case 223: return 13;
            case 255: return 14;
        }

        return 0;
    }

    static public int MaskToInternalIndex(byte mask)
    {
        switch (mask)
        {
            case 4:
            case 20:
            case 28:
            case 68:
            case 84:
            case 92:
            case 116:
            case 124:
            case 93:
            case 125:
            case 221:
            case 253:
                return 1;
            case 16:
            case 80:
            case 112:
            case 81:
            case 113:
            case 209:
            case 241:
            case 117:
            case 245:
            case 247:
                return 2;
            case 64:
            case 65:
            case 193:
            case 69:
            case 197:
            case 71:
            case 199:
            case 213:
            case 215:
            case 223:
                return 3;
        }
        return 0;
    }
}
