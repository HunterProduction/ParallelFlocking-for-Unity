
using UnityEngine;

public class FPSMouseLook : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float sensitivity = 50f;
    [SerializeField] private Vector2 axisSensWeight = Vector2.one;
    [Range(0,1)]
    [SerializeField] private float smoothness = 0f;
    [Range(0,90)]
    [SerializeField] private int upDownRange=90;

    public bool lockLook;

    private SimulationControls _controls;
    private Vector2 _rotation, _mouseDelta, _smoothMouseDelta;

    private void Awake()
    {
        _controls = new SimulationControls();
        
        if (mainCamera != null) return;
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogWarning("FPSMouseLook: MainCamera not set.");
        }
        
        
    }

    private void OnEnable()
    {
        _rotation = mainCamera.transform.eulerAngles;
        _controls.Looking.Enable();
    }

    private void OnDisable()
    {
        _controls.Looking.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if(lockLook)
            return;
        _mouseDelta = _controls.Looking.Look.ReadValue<Vector2>() * (sensitivity*axisSensWeight);
        _mouseDelta.y = -_mouseDelta.y;
        
        _smoothMouseDelta.x = Mathf.Lerp(_smoothMouseDelta.x, _mouseDelta.x, 1f-smoothness);
        _smoothMouseDelta.y = Mathf.Lerp(_smoothMouseDelta.y, _mouseDelta.y, 1f-smoothness);

        _rotation.x += _smoothMouseDelta.y * Time.deltaTime; 
        _rotation.x = Mathf.Clamp(_rotation.x, -upDownRange, upDownRange);
        
        _rotation.y += _smoothMouseDelta.x * Time.deltaTime;

        mainCamera.transform.rotation = Quaternion.Euler(_rotation);
    }
}
