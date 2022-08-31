using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;

public class ButtonHandlers : MonoBehaviour
{
    TextMeshPro feedbackText;
    public GameObject DeviceConnectButton;
    public GameObject ServiceButton;
    public GameObject CharacteristicsButton;
    private Vector3 _lastPos = new Vector3(0.03f, 0f, 0.01f);
    [SerializeField] private GridObjectCollection[] _grid;
    bool isScanningDevices;
    bool isScanningServices;
    bool isScanningCharacteristics;
    bool isSubscribed;
    Dictionary<string, Dictionary<string, string>> devices = new Dictionary<string, Dictionary<string, string>>();

    Dictionary<string, Dictionary<string, string>>
        deviceServices = new Dictionary<string, Dictionary<string, string>>();

    Dictionary<string, Dictionary<string, string>> serviceCharacteristics =
        new Dictionary<string, Dictionary<string, string>>();

    string selectedDeviceId;

    // A unity defined script method.  Called when the script object is first created.
    public void Start()
    {
        _grid = GameObject.FindObjectsOfType<GridObjectCollection>();
    }

    void Update()
    {
        BleApi.ScanStatus status;
        if (isScanningDevices)
        {
            BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
            do
            {
                status = BleApi.PollDevice(ref res, false);
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
                        devices[res.id]["isConnectable"] == "True")
                    {
                        // add new device to list
                        GameObject ins = Instantiate(DeviceConnectButton, GameObject.Find("Grid").transform, true);
                        ins.name = res.id;
                        ins.transform.position = new Vector3(_lastPos.x, _lastPos.y - 0.01f, _lastPos.z);
                        _lastPos.y -= 0.01f;
                        _grid[2].UpdateCollection();
                        // ins.transform.GetChild(0).GetComponent<TextMeshPro>().text = devices[res.id]["name"];
                        feedbackText = ins.GetComponentInChildren<TextMeshPro>();
                        feedbackText.SetText(devices[res.id]["name"]);
                        // ins.transform.GetChild(1).GetComponent<TextMeshPro>().text = res.id;
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    isScanningDevices = false;
                    //deviceScanButtonText.text = "Scan devices";
                    //deviceScanStatusText.text = "finished";
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
                    if (!deviceServices.ContainsKey(res.uuid))
                    {
                        deviceServices[res.uuid] = new Dictionary<string, string>()
                        {
                            { "name", UuidConverter.ConvertUuidToName(Guid.Parse(res.uuid)) }
                        };
                    }

                    if (deviceServices[res.uuid]["name"] != "")
                    {
                        GameObject ins = Instantiate(ServiceButton, GameObject.Find("ServiceGrid").transform, true);
                        ins.name = res.uuid;
                        _grid[0].UpdateCollection();
                        feedbackText = ins.GetComponentInChildren<TextMeshPro>();
                        feedbackText.SetText(deviceServices[res.uuid]["name"]);
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    isScanningServices = false;
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }

        if (isScanningCharacteristics)
        {
            BleApi.Characteristic res = new BleApi.Characteristic();
            do
            {
                status = BleApi.PollCharacteristic(out res, false);
                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    if (!serviceCharacteristics.ContainsKey(res.uuid))
                    {
                        serviceCharacteristics[res.uuid] = new Dictionary<string, string>()
                        {
                            { "name", UuidConverter.ConvertUuidToName(Guid.Parse(res.uuid)) }
                        };
                    }

                    if (serviceCharacteristics[res.uuid]["name"] != "")
                    {
                        GameObject ins = Instantiate(CharacteristicsButton,
                            GameObject.Find("CharacteristicsGrid").transform, true);
                        ins.name = res.uuid;
                        _grid[1].UpdateCollection();
                        feedbackText = ins.GetComponentInChildren<TextMeshPro>();
                        feedbackText.SetText(serviceCharacteristics[res.uuid]["name"]);
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    isScanningCharacteristics = false;
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }

        if (BLEManager.Instance.getSubscribed())
        {
            BleApi.BLEData res = new BleApi.BLEData();
            while (BleApi.PollData(out res, false))
            {
                Debug.Log("[DEBUG] EE value is " + BitConverter.ToSingle(res.buf, 0));
            }
        }
    }

    public void OnEnumerateClicked()
    {
        isScanningDevices = true;
        BleApi.StartDeviceScan();
    }

    public void OnConnectClicked()
    {
        isScanningServices = true;
        BleApi.ScanServices(BLEManager.Instance.GetDeviceId());
    }

    public void ShowCharacteristics()
    {
        isScanningCharacteristics = true;
        isScanningServices = false;
        isScanningDevices = false;
        BleApi.ScanCharacteristics(BLEManager.Instance.GetDeviceId(), BLEManager.Instance.GetServiceId());
    }

    public void SetDeviceId(GameObject data)
    {
        BLEManager.Instance.SetDeviceId(data.name);
        GameObject.Find("Connect").GetComponent<Interactable>().enabled = true;
    }

    public void SetServiceId(GameObject data)
    {
        BLEManager.Instance.SetServiceId(data.name);
        GameObject.Find("Connect").GetComponent<Interactable>().enabled = false;
        GameObject.Find("ShowCharacs").GetComponent<Interactable>().enabled = true;
    }

    public void Read(GameObject data)
    {
        if (!BLEManager.Instance.getSubscribed())
        {
            BLEManager.Instance.setSubscribed(true);
            Subscribe(data);
        }
        var toSend = new BleApi.BLEData();
        toSend.deviceId = BLEManager.Instance.GetDeviceId();
        toSend.serviceUuid = BLEManager.Instance.GetServiceId();
        toSend.characteristicUuid = data.name;
        BleApi.ReadData(toSend);
    }

    public void Subscribe(GameObject data)
    {
        // no error code available in non-blocking mode
        BleApi.SubscribeCharacteristic(BLEManager.Instance.GetDeviceId(), BLEManager.Instance.GetServiceId(), data.name,
            false);
    }

    public void sendCommand(string command)
    {
    }
}