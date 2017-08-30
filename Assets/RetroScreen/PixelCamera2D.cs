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
    protected int previousScreenWidth = 0;
    protected int previousScreenHeight = 0;

    protected Camera _camera;

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        if (Screen.width != previousScreenWidth || Screen.height != previousScreenHeight)
            Setup();
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