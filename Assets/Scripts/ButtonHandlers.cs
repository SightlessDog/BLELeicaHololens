using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;

public class ButtonHandlers : MonoBehaviour
{
    public GameObject DeviceConnectButton;
    public GameObject ServiceButton;
    public GameObject CharacteristicsButton;
    public GameObject CommandsList;
    private Vector3 _lastPos = new Vector3(0.03f, 0f, 0.01f);
    [SerializeField] private GameObject devicesList;
    [SerializeField] private GameObject serviceList;
    [SerializeField] private GameObject characteristicsList;
    [SerializeField] private GameObject deviceResponse;
    bool isScanningDevices;
    bool isScanningServices;
    bool isScanningCharacteristics;
    bool writingToDevice;
    bool deviceShown;
    TextMeshPro feedbackText;
    public static event Action<State> onStateChanged;

    readonly Dictionary<string, Dictionary<string, string>> devices =
        new Dictionary<string, Dictionary<string, string>>();

    // A unity defined script method.  Called when the script object is first created.
    public void Start()
    {
        UpdateAppState(State.STARTED);
        CommandsList.gameObject.SetActive(false);
        BLEManager.Instance.SetCommandsList(CommandsList);
    }

    void Update()
    {
        BleApi.ScanStatus status;
        if (isScanningDevices)
        {
            BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
            do
            {
                status = BleApi.PollDevice(out res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    if (!devices.ContainsKey(res.id))
                        devices[res.id] = new Dictionary<string, string>()
                        {
                            { "name", "" },
                            { "isConnectable", "False" }
                        };
                    if (res.nameUpdated)
                        devices[res.id]["name"] = res.name;
                    if (res.isConnectableUpdated)
                        devices[res.id]["isConnectable"] = res.isConnectable.ToString();
                    // consider only devices which have a name and which are connectable
                    if (devices[res.id]["name"] != "" && devices[res.id]["name"].Contains("DISTO") &&
                        devices[res.id]["isConnectable"] == "True" && !deviceShown)
                    {
                        deviceShown = true;
                        // add new device to list
                        GameObject ins = Instantiate(DeviceConnectButton, devicesList.transform, true);
                        ins.name = res.id;
                        ins.transform.position = new Vector3(_lastPos.x, _lastPos.y - 0.01f, _lastPos.z);
                        _lastPos.y -= 0.01f;
                        devicesList.GetComponent<GridObjectCollection>().UpdateCollection();
                        feedbackText = ins.GetComponentInChildren<TextMeshPro>();
                        feedbackText.SetText(devices[res.id]["name"]);
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    isScanningDevices = false;
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }

        if (isScanningServices)
        {
            BleApi.Service res = new BleApi.Service();
            do
            {
                status = BleApi.PollService(out res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    BLEManager.Instance.HandleComingServiceData(res.uuid);
                    // TODO try to use less calls here, but for the moment all good
                    if (BLEManager.Instance.getServiceList()[res.uuid]["name"] != "")
                    {
                        GameObject ins = Instantiate(ServiceButton, serviceList.transform, true);
                        ins.name = res.uuid;
                        serviceList.GetComponent<GridObjectCollection>().UpdateCollection();
                        feedbackText = ins.GetComponentInChildren<TextMeshPro>();
                        feedbackText.SetText(BLEManager.Instance.getServiceList()[res.uuid]["name"]);
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    UpdateAppState(State.CONNECTED);
                    isScanningServices = false;
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }

        if (isScanningCharacteristics)
        {
            BleApi.Characteristic res = new BleApi.Characteristic();
            Debug.Log("[DEBUG] EE characs");
            do
            {
                status = BleApi.PollCharacteristic(out res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    BLEManager.Instance.HandleComingCharacteristicsData(res.uuid);
                    if (BLEManager.Instance.getCharacteristicsList()[res.uuid]["name"] != "")
                    {
                        GameObject ins = Instantiate(CharacteristicsButton,
                            characteristicsList.transform, true);
                        ins.name = res.uuid;
                        characteristicsList.GetComponent<GridObjectCollection>().UpdateCollection();
                        feedbackText = ins.GetComponentInChildren<TextMeshPro>();
                        feedbackText.SetText(BLEManager.Instance.getCharacteristicsList()[res.uuid]["name"]);
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    UpdateAppState(State.CHARACTERISTICSSHOWN);
                    isScanningCharacteristics = false;
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }

        if (BLEManager.Instance.getSubscribed())
        {
            BleApi.BLEData res = new BleApi.BLEData();
            while (BleApi.PollData(out res, false))
            {
                if (BLEManager.Instance.getCustomLeicaValue())
                {
                    float value = BitConverter.ToSingle(res.buf, 0);
                    ImageProcessor.Instance.TriggerShot(value);
                    NotificationManager.Instance.SetNewNotification("Locating Leica in the space, please hold your head still for a second");
                    //TODO: Make sure the unit is dynamic
                    deviceResponse.GetComponent<TextMeshPro>()
                        .SetText("Value Read from the device is : " + value + " m.");
                }
                else
                {
                    string value = Encoding.Unicode.GetString(res.buf, 0, res.size);
                    deviceResponse.GetComponent<TextMeshPro>().SetText("Value Read from the device is : " + value);
                    BLEManager.Instance.setSubscribed(false);
                }
            }
        }
        
        //ShapeBuilder.Instance.BuildShape();
    }

    public void OnEnumerateClicked()
    {
        isScanningDevices = true;
        BleApi.StartDeviceScan();
        UpdateAppState(State.ENUMERATING);
    }

    public void OnConnectClicked()
    {
        isScanningServices = true;
        foreach (Transform t in serviceList.transform)
        {
            GameObject.Destroy(t.gameObject);
        }

        BleApi.ScanServices(BLEManager.Instance.GetDeviceId());
        UpdateAppState(State.CONNECTING);
    }

    public void ShowCharacteristics()
    {
        isScanningCharacteristics = true;
        isScanningServices = false;
        isScanningDevices = false;
        characteristicsList = GameObject.Find("CharacteristicsGrid");
        foreach (Transform t in characteristicsList.transform)
        {
            if (t)
                Destroy(t.gameObject);
            else continue;
        }
        BleApi.ScanCharacteristics(BLEManager.Instance.GetDeviceId(), BLEManager.Instance.GetServiceId());
        UpdateAppState(State.SHOWINGCHARACTERISTICS);
    }

    public void SetDeviceId(GameObject data)
    {
        BLEManager.Instance.SetDeviceName(data.GetComponentInChildren<TextMeshPro>().text); 
        BLEManager.Instance.SetDeviceId(data.name);
        GameObject.Find("Connect").GetComponent<Interactable>().enabled = true;
        UpdateAppState(State.ENUMERATED);
    }

    public void SetServiceId(GameObject data)
    {
        BLEManager.Instance.SetServiceId(data.name);
        GameObject.Find("Connect").GetComponent<Interactable>().enabled = false;
        GameObject.Find("Disconnect").GetComponent<Interactable>().enabled = true;
        // If it's "disto" device then we need another way to deal with the data we get
        if (BLEManager.Instance.getServiceList()[data.name]["name"].ToUpper().Contains("DISTO"))
        {
            BLEManager.Instance.setSubscribed(false);
            BLEManager.Instance.setCustomLeicaValue(true);
        }
        else
        {
            BLEManager.Instance.setSubscribed(false);
            BLEManager.Instance.setCustomLeicaValue(false);
        }
        ShowCharacteristics();
    }

    public void SetCharacteristicId(GameObject data)
    {
        BLEManager.Instance.setCharacteristicId(data.name);
        if (BLEManager.Instance.getCharacteristicsList()[data.name]["name"].ToUpper().Contains("COMMAND"))
        {
            if (!writingToDevice)
            {
                writingToDevice = true;
                BLEManager.Instance.getCommandsList().gameObject.SetActive(true);
                Read(data);
            }
            else
            {
                writingToDevice = false;
                BLEManager.Instance.getCommandsList().gameObject.SetActive(false);
                BLEManager.Instance.setSubscribed(false);
            }
        }
        else
        {
            writingToDevice = false;
            BLEManager.Instance.getCommandsList().gameObject.SetActive(false);
            Read(data);
        }
    }

    public void Read(GameObject data)
    {
        if (!BLEManager.Instance.getSubscribed() && BLEManager.Instance.getCustomLeicaValue())
        {
            BLEManager.Instance.setSubscribed(true);
            Subscribe(data.name);
        }
        else if (!BLEManager.Instance.getSubscribed() && !BLEManager.Instance.getCustomLeicaValue())
        {
            BLEManager.Instance.setSubscribed(true);
            var toSend = new BleApi.BLEData();
            toSend.deviceId = BLEManager.Instance.GetDeviceId();
            toSend.serviceUuid = BLEManager.Instance.GetServiceId();
            toSend.characteristicUuid = data.name;
            BleApi.ReadData(toSend);
        }
    }

    public void Subscribe(string data)
    {
        BleApi.SubscribeCharacteristic(BLEManager.Instance.GetDeviceId(), BLEManager.Instance.GetServiceId(),
            data,
            false);
    }
    
    public void SendCommand(GameObject data)
    {
        string command = data.name;
        Commands res;
        Commands.TryParse(command, out res);
        if (res == Commands.LaserOff)
        {
            NotificationManager.Instance.SetNewNotification("Laser went off");
        }
        if (res == Commands.LaserOn)
        {
            NotificationManager.Instance.SetNewNotification("Laser went on");
        }
        string value = Util.GetEnumMemberAttrValue(typeof(Commands), res);
        byte[] payload = Encoding.ASCII.GetBytes(value);
        BleApi.BLEData toSend = new BleApi.BLEData();
        toSend.buf = new byte[512];
        toSend.size = (short)payload.Length;
        toSend.deviceId = BLEManager.Instance.GetDeviceId();
        toSend.serviceUuid = BLEManager.Instance.GetServiceId();
        toSend.characteristicUuid = BLEManager.Instance.getCharacteristicId();
        for (int i = 0; i < payload.Length; i++)
            toSend.buf[i] = payload[i];
        
        if (data.name.ToUpper() == "DISTANCE")
        {
            foreach (var pair in BLEManager.Instance.getCharacteristicsList())
            {
                if (pair.Value.ContainsValue(data.name))
                {
                    Subscribe(pair.Key);
                    Debug.Log("Pair value is " + pair.Key);
                }
            }    
        }
        // no error code available in non-blocking mode
        BleApi.SendData(in toSend, false);
    }

    public void Disconnect()
    {
        GameObject.Find("Disconnect").GetComponent<Interactable>().enabled = false;
        BleApi.Quit();
        UpdateAppState(State.DISCONNECTED);
    }

    public void UpdateAppState(State state)
    {
        onStateChanged?.Invoke(state);

        if (state == State.DISCONNECTED)
        {
            foreach (Transform t in serviceList.transform)
            {
                Destroy(t.gameObject);
            }

            foreach (Transform t in characteristicsList.transform)
            {
                Destroy(t.gameObject);
            }
        }
    }

    private void OnApplicationQuit()
    {
        BleApi.Quit();
    }
}

public enum State
{
    STARTED,
    ENUMERATING,
    ENUMERATED,
    CONNECTING,
    CONNECTED,
    SERVICESELECTED,
    DISCONNECTING,
    DISCONNECTED,
    SHOWINGCHARACTERISTICS,
    CHARACTERISTICSSHOWN
}