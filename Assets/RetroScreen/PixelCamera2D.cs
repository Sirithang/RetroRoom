﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelCamera2D : MonoBehaviour
{
    public enum ScreenTransitionState
    {
        START,
        END
    }

    public enum TransitionType
    {
        INSTANT,
        MOVING,
        FADE
    }

    public delegate void ScreenTransition(ScreenTransitionState state);

    static protected PixelCamera2D s_instance = null;
    static public PixelCamera2D Instance { get { return s_instance; } }

    public Transform follow;
    public TransitionType transitionType;
    public bool freezeTimeDuringTransition = false;
    public float transitionSpeed = 10.0f;

    public ScreenTransition onScreenTransition = null;

    protected int previousScreenWidth = 0;
    protected int previousScreenHeight = 0;

    protected Camera _camera;

    protected int currentRoom = -1;
    protected bool _inTransition = false;
    protected float _pixelSize;
    protected float _previousTimeScale;

    private void Awake()
    {
        s_instance = this;
    }

    private void Start()
    {
        Setup();

#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

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
        else if(cell.room != currentRoom)
        {
            if (transitionType != TransitionType.INSTANT)
            {
                _inTransition = true;

                _previousTimeScale = Time.timeScale;
                Time.timeScale = freezeTimeDuringTransition ? 0.0f : Time.timeScale;
            }

            currentRoom = cell.room;
            if(onScreenTransition != null)
                onScreenTransition(ScreenTransitionState.START);
        }

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

        //position.x = Mathf.RoundToInt(position.x / _pixelSize) * _pixelSize;
        //position.y = Mathf.RoundToInt(position.y / _pixelSize) * _pixelSize;

        if (_inTransition)
        {
            switch(transitionType)
            {
                case TransitionType.MOVING:
                    MovingTransition(position);
                    break;
                case TransitionType.FADE:
                    break;
                default:
                    break;
            }
        }
        else
        {
            transform.position = position;
        }
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

    protected void MovingTransition(Vector3 position)
    {
        transform.position = Vector3.MoveTowards(transform.position, position, transitionSpeed * Time.unscaledDeltaTime);
        if (position == transform.position)
        {
            _inTransition = false;

            Time.timeScale = _previousTimeScale;

            if (onScreenTransition != null)
                onScreenTransition(ScreenTransitionState.END);
        }
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