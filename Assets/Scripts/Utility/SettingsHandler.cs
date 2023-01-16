using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsHandler : MonoBehaviour
{
    public bool vSync = false;
    public Vector2Int resolution = new (1920, 1080);
    public bool fullScreen = true;

    private void Awake()
    {
        QualitySettings.vSyncCount = vSync ? 1 : 0;
        Application.targetFrameRate = 600;
        Screen.SetResolution(resolution.x, resolution.y, fullScreen);
    }
}
