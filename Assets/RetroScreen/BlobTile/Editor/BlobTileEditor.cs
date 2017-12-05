using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(BlobTile))]
public class BlobTileEditor : Editor
{
    //This match the tile in the 7x7 pattern to their entry in the blob tile
    readonly int[,] spriteToEntrieIdxAndRotation =
    {
        {0,0}, {1,1}, {6,1}, {8,1}, {7,1}, {2,2}, {0,0},
        {1,2}, {2,1}, {10,0}, {13,3}, {8,2}, {5,0}, {1,3},
        {7,0}, {10,2}, {9,0}, {7,3}, {12,1}, {11,1}, {3,2},
        {8,0}, {13,1}, {6,2}, {3,1}, {13,0}, {13,2}, {7,2},
        {6,0}, {8,3}, {10,3}, {11,0}, {14,0}, {11,2}, {5,2},
        {2,0}, {5,1}, {10,1}, {12,0}, {11,3}, {3,3}, {4,0},
        {0,0}, {1,0}, {3,0}, {6,3}, {5,3}, {4,1}, {2,3}
    };

    readonly Color _transparent = new Color(0, 0, 0, 0);

    protected BlobTile _target;
    protected Texture _schemaTexture;
    protected bool _hover = false;
    protected int _hoverX;
    protected int _hoverY;

    protected int _selectedIdx = 0;

    Rect _previewRect;
    Rect _selectionRectangle;

    void OnEnable()
    {
        _target = target as BlobTile;
        _schemaTexture = EditorGUIUtility.Load("blobtileschema.png") as Texture;
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    public override void OnInspectorGUI()
    {
        int schemaTileSize = _schemaTexture.width / 7;

        if (GUILayout.Button("Assign from texture"))
        {
            EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "", 0);
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(_schemaTexture);

        if(Event.current.type == EventType.Repaint)
        {
            _previewRect = GUILayoutUtility.GetLastRect();

            Vector2 localPosition = Event.current.mousePosition - _previewRect.min;

            if (localPosition.x < 0 || localPosition.y < 0 || localPosition.x > _schemaTexture.width || localPosition.y > _schemaTexture.height)
                _hover = false;
            else
            {
                _hover = true;
                _hoverX = Mathf.FloorToInt(localPosition.x / schemaTileSize);
                _hoverY = Mathf.FloorToInt(localPosition.y / schemaTileSize);

                Handles.DrawSolidRectangleWithOutline(new Rect(_previewRect.xMin + _hoverX * schemaTileSize, _previewRect.yMin + _hoverY * schemaTileSize, schemaTileSize, schemaTileSize), _transparent, Color.green);
            }
        }

        if (Event.current.type == EventType.MouseUp)
        {
            if (_hover)
            {
                _selectedIdx = _hoverY * 7 + _hoverX;
                _selectionRectangle = new Rect(_previewRect.xMin + _hoverX * schemaTileSize, _previewRect.yMin + _hoverY * schemaTileSize, schemaTileSize, schemaTileSize);
            }
            else
            {
                _selectedIdx = -1;
            }

            Repaint();
        }

        if(_selectedIdx != -1)
        {
            Handles.DrawSolidRectangleWithOutline(_selectionRectangle, _transparent, Color.red);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if(Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorUpdated")
        {
            Texture2D tex = EditorGUIUtility.GetObjectPickerObject() as Texture2D;

            string spriteSheet = AssetDatabase.GetAssetPath(tex);
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();

            int spriteSize = tex.width / 7;

            //if(sprites.Length >= 46)
            {
                for(int i = 0; i < 15; ++i)
                {
                    for(int k = 0; k < 4; ++k)
                    {
                        _target.entries[i].rotation[k] = 0;
                        _target.entries[i].sprite[k] = null;
                    }
                }

                for(int i =0; i < sprites.Length; ++i)
                {
                    int x = Mathf.FloorToInt((sprites[i].textureRect.x + 2) / spriteSize);
                    int y = 6 - Mathf.FloorToInt((sprites[i].textureRect.y + 2) / spriteSize);

                    int idx = y * 7 + x;
                    int entry = spriteToEntrieIdxAndRotation[idx, 0];
                    int rotation = spriteToEntrieIdxAndRotation[idx, 1];

                    _target.entries[entry].sprite[rotation] = sprites[i];
                    _target.entries[entry].rotation[rotation] = 0;
                }
            }
            
            EditorUtility.SetDirty(target);
        }

        //display data for the selected tile
        
        if(_selectedIdx != -1)
        {
            int entry = spriteToEntrieIdxAndRotation[_selectedIdx, 0];
            int rotation = spriteToEntrieIdxAndRotation[_selectedIdx, 1];

            EditorGUILayout.BeginHorizontal();

            _target.entries[entry].sprite[rotation] = EditorGUILayout.ObjectField(_target.entries[entry].sprite[rotation], typeof(Sprite), false, new GUILayoutOption[] { GUILayout.Width(64), GUILayout.Height(64) }) as Sprite;
            _target.entries[entry].rotation[rotation] = EditorGUILayout.FloatField("Rotation", _target.entries[entry].rotation[rotation]);

           EditorGUILayout.EndHorizontal();
        }     
    }
}
