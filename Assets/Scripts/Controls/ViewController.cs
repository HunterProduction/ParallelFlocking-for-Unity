using UnityEngine;
using UnityEngine.InputSystem;

public class ViewController : MonoBehaviour
{
    [SerializeField] private GameObject simulationSettingsView;
    [SerializeField] private FPSMouseLook mouseLook;

    private SimulationControls _input;
    
    private void Awake()
    {
        _input = new SimulationControls();
        _input.UI.Enable();
    }

    private void OnEnable()
    {
        _input.UI.Tab.performed += OnTabPerformed;
        //_input.UI.Esc.performed += OnEscPerformed;
    }

    private void OnDisable()
    {
        _input.UI.Tab.performed -= OnTabPerformed;
        //_input.UI.Esc.performed -= OnEscPerformed;
    }

    private void OnTabPerformed(InputAction.CallbackContext context)
    {
        simulationSettingsView.SetActive(!simulationSettingsView.activeSelf);
        mouseLook.lockLook = simulationSettingsView.activeSelf;
    }

    /*
    private void OnEscPerformed(InputAction.CallbackContext context)
    {
        
    }
    */
}
