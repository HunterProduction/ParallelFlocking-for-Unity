using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimulationController : MonoBehaviour
{
    [Header("Handler Reference")]
    [SerializeField] private FlockGPUHandler flockHandler;

    public Flock Flock => flockHandler.flock;
    public bool SimulationRunning => flockHandler.gameObject.activeSelf;

    public UnityEvent onSimulationStarted, onSimulationStopped;

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
