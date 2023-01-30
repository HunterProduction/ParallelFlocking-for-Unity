using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFlockSettings : MonoBehaviour
{
    [Header("Handler Reference")]
    [SerializeField] private SimulationController simController;
    
    [Header("UI Fields")]
    [SerializeField] private TMP_InputField flockRadius;
    [SerializeField] private TMP_InputField agentScale;
    [SerializeField] private TMP_InputField numAgents;
    [SerializeField] private TMP_InputField agentViewRange;
    [SerializeField] private TMP_InputField driveFactor;
    
    [SerializeField] private Toggle avoidanceActive;
    [SerializeField] private Slider avoidanceWeight;
    [SerializeField] private TMP_InputField avoidanceRadius;

    [SerializeField] private Toggle cohesionActive;
    [SerializeField] private Slider cohesionWeight;
    [SerializeField] private TMP_InputField cohesionRadius;
    
    [SerializeField] private Toggle alignmentActive;
    [SerializeField] private Slider alignmentWeight;
    [SerializeField] private TMP_InputField alignmentRadius;
    
    [SerializeField] private Toggle boundingSphereActive;
    [SerializeField] private Slider boundingSphereWeight;
    [SerializeField] private TMP_InputField boundingSphereRadius;
    
    private void OnEnable()
    {
        simController.onSimulationStarted.AddListener(()=>
        {
            numAgents.interactable = false;
            flockRadius.interactable = false;
        });
        simController.onSimulationStopped.AddListener(()=>
        {
            numAgents.interactable = true;
            flockRadius.interactable = true;
        });
        InitializeUIFields();
        SubscribeAllUIFields();
        simController.onHandlerChanged.AddListener(() =>
        {
            UnsubscribeAllUIFields();
            InitializeUIFields();
            SubscribeAllUIFields();
        });
    }

    private void OnDisable()
    {
        simController.onSimulationStarted.RemoveListener(()=>
        {
            numAgents.interactable = false;
            flockRadius.interactable = false;
        });
        simController.onSimulationStopped.RemoveListener(()=>
        {
            numAgents.interactable = true;
            flockRadius.interactable = true;
        });
        simController.onHandlerChanged.RemoveListener(() =>
        {
            UnsubscribeAllUIFields();
            InitializeUIFields();
            SubscribeAllUIFields();
        });
        UnsubscribeAllUIFields();
    }

    private void InitializeUIFields()
    {
        flockRadius.text = simController.Flock.flockRadius.ToString("0.0");
        agentScale.text = simController.Flock.agentScale.ToString("0.0");
        numAgents.text = simController.Flock.numAgents.ToString("0");
        agentViewRange.text = simController.Flock.agentViewRange.ToString("0.0");
        driveFactor.text = simController.Flock.driveFactor.ToString("0.0");

        avoidanceActive.isOn = simController.Flock.avoidanceParameters.active;
        avoidanceWeight.value = simController.Flock.avoidanceParameters.weight;
        avoidanceRadius.text = simController.Flock.avoidanceParameters.radius.ToString("0.0");
        
        cohesionActive.isOn = simController.Flock.cohesionParameters.active;
        cohesionWeight.value = simController.Flock.cohesionParameters.weight;
        cohesionRadius.text = simController.Flock.cohesionParameters.radius.ToString("0.0");
        cohesionRadius.interactable = false;
        
        alignmentActive.isOn = simController.Flock.alignmentParameters.active;
        alignmentWeight.value = simController.Flock.alignmentParameters.weight;
        alignmentRadius.text = simController.Flock.alignmentParameters.radius.ToString("0.0");
        alignmentRadius.interactable = false;
        
        boundingSphereActive.isOn = simController.Flock.boundingSphereParameters.active;
        boundingSphereWeight.value = simController.Flock.boundingSphereParameters.weight;
        boundingSphereRadius.text = simController.Flock.boundingSphereParameters.radius.ToString("0.0");
    }

    private void SubscribeAllUIFields()
    {
        flockRadius.onEndEdit.AddListener(val => simController.Flock.flockRadius = float.Parse(val));
        agentScale.onEndEdit.AddListener(val => simController.Flock.agentScale = float.Parse(val));
        numAgents.onEndEdit.AddListener(val => simController.Flock.numAgents = int.Parse(val));
        
        agentViewRange.onEndEdit.AddListener(val => simController.Flock.agentViewRange = float.Parse(val));
        agentViewRange.onEndEdit.AddListener(val =>
        {
            simController.Flock.cohesionParameters.radius = float.Parse(val);
            cohesionRadius.text = val;
        });
        agentViewRange.onEndEdit.AddListener(val =>
        {
            simController.Flock.alignmentParameters.radius = float.Parse(val);
            alignmentRadius.text = val;
        });
        
        driveFactor.onEndEdit.AddListener(val => simController.Flock.driveFactor = float.Parse(val));
        
        avoidanceActive.onValueChanged.AddListener(val => simController.Flock.avoidanceParameters.active = val);
        avoidanceWeight.onValueChanged.AddListener(val => simController.Flock.avoidanceParameters.weight = val);
        avoidanceRadius.onEndEdit.AddListener(val => simController.Flock.avoidanceParameters.radius = float.Parse(val));
        
        cohesionActive.onValueChanged.AddListener(val => simController.Flock.cohesionParameters.active = val);
        cohesionWeight.onValueChanged.AddListener(val => simController.Flock.cohesionParameters.weight = val);
        cohesionRadius.onEndEdit.AddListener(val => simController.Flock.cohesionParameters.radius = float.Parse(val));
        
        alignmentActive.onValueChanged.AddListener(val => simController.Flock.alignmentParameters.active = val);
        alignmentWeight.onValueChanged.AddListener(val => simController.Flock.alignmentParameters.weight = val);
        alignmentRadius.onEndEdit.AddListener(val => simController.Flock.alignmentParameters.radius = float.Parse(val));
        
        boundingSphereActive.onValueChanged.AddListener(val => simController.Flock.boundingSphereParameters.active = val);
        boundingSphereWeight.onValueChanged.AddListener(val => simController.Flock.boundingSphereParameters.weight = val);
        boundingSphereRadius.onEndEdit.AddListener(val => simController.Flock.boundingSphereParameters.radius = float.Parse(val));
    }

    private void UnsubscribeAllUIFields()
    {
        flockRadius.onEndEdit.RemoveAllListeners();
        agentScale.onEndEdit.RemoveAllListeners();
        numAgents.onEndEdit.RemoveAllListeners();
        agentViewRange.onEndEdit.RemoveAllListeners();
        driveFactor.onEndEdit.RemoveAllListeners();
        
        avoidanceActive.onValueChanged.RemoveAllListeners();
        avoidanceWeight.onValueChanged.RemoveAllListeners();
        avoidanceRadius.onEndEdit.RemoveAllListeners();
        
        cohesionActive.onValueChanged.RemoveAllListeners();
        cohesionWeight.onValueChanged.RemoveAllListeners();
        cohesionRadius.onEndEdit.RemoveAllListeners();
        
        alignmentActive.onValueChanged.RemoveAllListeners();
        alignmentWeight.onValueChanged.RemoveAllListeners();
        alignmentRadius.onEndEdit.RemoveAllListeners();
        
        boundingSphereActive.onValueChanged.RemoveAllListeners();
        boundingSphereWeight.onValueChanged.RemoveAllListeners();
        boundingSphereRadius.onEndEdit.RemoveAllListeners();
    }
    
}
