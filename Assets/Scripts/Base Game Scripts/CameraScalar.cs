using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScalar : MonoBehaviour
{
    private Board _board;
    public float cameraOffset;
    public float aspectRatio;
    public float padding = 2;

    void Start()
    {
        
        _board = FindObjectOfType<Board>();
        
        if (_board != null)
        {
            RepositionCamera(_board.width-1,_board.height-1);
        }
        
    }

    void RepositionCamera(float x, float y)
    {
        Vector3 tempPosition = new Vector3(x/2, y/2, cameraOffset);
        transform.position = tempPosition;
        if(_board.width >=_board.height)
        {
            Camera.main.orthographicSize = (_board.width  + padding-1) / aspectRatio;
        }
        else
        {
            Camera.main.orthographicSize = (_board.width + padding-1);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
