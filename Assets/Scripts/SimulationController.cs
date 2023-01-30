using UnityEngine;
using UnityEngine.Events;

public class SimulationController : MonoBehaviour
{
    [Header("Handler Reference")]
    public FlockHandler flockHandler;

    public Flock Flock => flockHandler.flock;
    public bool SimulationRunning => flockHandler.gameObject.activeSelf;

    public UnityEvent onSimulationStarted, onSimulationStopped, onHandlerChanged;

    public void StartSimulation()
    {
        flockHandler.gameObject.SetActive(true);
        onSimulationStarted.Invoke();
    }

    public void StopSimulation()
    {
        flockHandler.gameObject.SetActive(false);
        onSimulationStopped.Invoke();
    }
}
