using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class DifferentialScrolling : MonoBehaviour
{
    public float fixFactor = 1.0f;
    
    protected Vector3 _lastCameraPosition;

    void Start()
    {
        _lastCameraPosition = PixelCamera2D.Instance.transform.position;
    }
    
    private void LateUpdate()
    {
        Vector3 movement = PixelCamera2D.Instance.transform.position - _lastCameraPosition;

        transform.position += movement * fixFactor;
        
        _lastCameraPosition = PixelCamera2D.Instance.transform.position;
    }
}
