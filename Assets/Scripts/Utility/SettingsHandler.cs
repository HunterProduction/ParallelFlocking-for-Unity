using UnityEngine;
using UnityEngine.Profiling;

public class SettingsHandler : MonoBehaviour
{
    public bool vSync = false;
    public Vector2Int resolution = new (1920, 1080);
    public bool fullScreen = true;

    private void Awake()
    {
        Profiler.maxUsedMemory = 1073741824;
        QualitySettings.vSyncCount = vSync ? 1 : 0;
        Application.targetFrameRate = 600;
        Screen.SetResolution(resolution.x, resolution.y, fullScreen);
    }
}
