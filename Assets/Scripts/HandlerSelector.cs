using TMPro;
using UnityEngine;

public class HandlerSelector : MonoBehaviour
{
    [SerializeField] private SimulationController simController;
    [SerializeField] private FlockHandler handlerGPU;
    [SerializeField] private FlockHandler handlerCPU;
    [SerializeField] private TMP_Dropdown selectorReference;
    private void OnEnable()
    {
        selectorReference.onValueChanged.AddListener(SelectHandler);
    }

    private void OnDisable()
    {
        selectorReference.onValueChanged.RemoveListener(SelectHandler);
    }

    private void SelectHandler(int option)
    {
        switch (option)
        {
            case 0:
                simController.flockHandler = handlerGPU;
                break;
            case 1:
                simController.flockHandler = handlerCPU;
                break;
        }
        simController.onHandlerChanged.Invoke();
    }
}
