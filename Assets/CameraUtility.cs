using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraUtility
{
    public static Bounds GetCameraBounds(Camera camera)
    {
        float screenAspect = (float)Screen.width/(float)Screen.height;
        float height = Camera.main.orthographicSize * 2;
        float width = screenAspect * height;
        return new Bounds(camera.transform.position, new Vector3(width, height, 0));
    }
}
