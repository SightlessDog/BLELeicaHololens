using UnityEngine;

public class ButtonStyler : MonoBehaviour
{
    [SerializeField] private Material activeState;
    [SerializeField] private Material disabledState;
    [SerializeField] private Material connectState;
    [SerializeField] private Material disconnectState;
    [SerializeField] private GameObject connectButton;
    [SerializeField] private GameObject disconnectButton;
    [SerializeField] private GameObject enumerateButton;
    [SerializeField] private GameObject characteristicsButton;

    private void Awake()
    {
        ButtonHandlers.onStateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        ButtonHandlers.onStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(State state)
    {
        if (state == State.STARTED)
        {
            enumerateButton.GetComponent<MeshRenderer>().material = activeState;
        } else if (state == State.ENUMERATED)
        {
            enumerateButton.GetComponent<MeshRenderer>().material = disabledState;
            if (BLEManager.Instance.GetDeviceId() != null)
            {
                connectButton.GetComponent<MeshRenderer>().material = connectState;
            }
        } else if (state == State.CONNECTED)
        {
            connectButton.GetComponent<MeshRenderer>().material = disabledState;
            disconnectButton.GetComponent<MeshRenderer>().material = disconnectState;
            if (BLEManager.Instance.GetServiceId() != null)
            {
                characteristicsButton.GetComponent<MeshRenderer>().material = activeState;
            }
        } else if (state == State.DISCONNECTED)
        {
            disconnectButton.GetComponent<MeshRenderer>().material = disabledState;
            enumerateButton.GetComponent<MeshRenderer>().material = disabledState;
            connectButton.GetComponent<MeshRenderer>().material = connectState;
        }
    }
}
