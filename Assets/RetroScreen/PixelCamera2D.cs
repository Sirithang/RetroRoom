using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelCamera2D : MonoBehaviour
{
    public Transform follow;

    protected int previousScreenWidth = 0;
    protected int previousScreenHeight = 0;

    protected Camera _camera;

    protected int currentRoom = -1;
    protected float _pixelSize;

    private void Start()
    {
        Setup();

        RoomCell cell = RoomManager.Instance.GetCellFromWorld(follow.transform.position);

        if (cell == null)
            Debug.LogError("target outside of the world");
        else
            currentRoom = cell.room;
    }

    private void Update()
    {
        if (Screen.width != previousScreenWidth || Screen.height != previousScreenHeight)
            Setup();
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        if (follow == null)
            return;

        RoomCell cell = RoomManager.Instance.GetCellFromWorld(follow.transform.position);

        if (cell == null)
            Debug.LogError("target outside of the world");
        else
            currentRoom = cell.room;

        Vector3 position = follow.position - Vector3.forward * 5.0f;

        Rect cameraRect = new Rect(position.x - _camera.orthographicSize * _camera.aspect, position.y - _camera.orthographicSize, _camera.orthographicSize * 2.0f * _camera.aspect, _camera.orthographicSize * 2.0f);

        if(currentRoom != -1)
        {
            Rect roomRect = RoomManager.Instance.rooms[currentRoom].worldRect;

            if (cameraRect.xMin < roomRect.xMin)
                cameraRect.position = new Vector2(roomRect.xMin, cameraRect.position.y);

            if(cameraRect.yMin < roomRect.yMin)
                cameraRect.position = new Vector2(cameraRect.position.x, roomRect.yMin);

            if (cameraRect.xMax > roomRect.xMax)
                cameraRect.position = new Vector2(roomRect.xMax - cameraRect.width, cameraRect.position.y);

            if (cameraRect.yMax > roomRect.yMax)
                cameraRect.position = new Vector2(cameraRect.position.x, roomRect.yMax - cameraRect.height);

            float z = position.z;
            position = cameraRect.center;
            position.z = z;
        }

        position.x = Mathf.RoundToInt(position.x / _pixelSize) * _pixelSize;
        position.y = Mathf.RoundToInt(position.y / _pixelSize) * _pixelSize;

        transform.position = position;
    }

    public void Setup()
    {
        int referenceWidth = RetroScreenSettings.instance.width;
        int referenceHeight = RetroScreenSettings.instance.height;
        int ppu = RetroScreenSettings.instance.pixelPerUnits;

        _camera = GetComponent<Camera>();

        previousScreenWidth = Screen.width;
        previousScreenHeight = Screen.height;

        int xZoom = Mathf.FloorToInt(Screen.width / referenceWidth);
        int yZoom = Mathf.FloorToInt(Screen.height / referenceHeight);

        _camera.orthographic = true;
        
        Rect screenRect = new Rect();
        int usedZoom;

        int heighToUse = referenceHeight;

        if (xZoom < yZoom) 
        { // fit along width
            float ratio = referenceHeight / (float)referenceWidth;

            int widthToUse = referenceWidth;

            screenRect.width = Mathf.Floor(xZoom * widthToUse);
            screenRect.height = Mathf.Floor(ratio * screenRect.width);

            usedZoom = xZoom;
        }
        else
        { // fit alone height
            float ratio = referenceWidth / (float)referenceHeight;

            screenRect.height = Mathf.Floor(yZoom * heighToUse);
            screenRect.width = Mathf.Floor(ratio * screenRect.height);

            usedZoom = yZoom;
        }

        screenRect.x = Mathf.Floor((Screen.width - screenRect.width) * 0.5f);
        screenRect.y = Mathf.Floor((Screen.height - screenRect.height) * 0.5f);

        _camera.pixelRect = screenRect;
        _camera.orthographicSize = (heighToUse / (float)ppu) * 0.5f;

        _pixelSize = 1.0f / RetroScreenSettings.instance.pixelPerUnits;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PixelCamera2D))]
public class PixelCamEditor : Editor
{
    PixelCamera2D _camera;

    private void OnEnable()
    {
        _camera = target as PixelCamera2D;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        if(EditorGUI.EndChangeCheck())
        {
            _camera.Setup();
        }
    }
}
#endif