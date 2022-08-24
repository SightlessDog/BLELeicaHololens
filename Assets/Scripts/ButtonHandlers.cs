using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;
using System;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Utilities;
using TestApp.Sample;
using TMPro;
#if UNITY_WSA && !UNITY_EDITOR
using UnityUWPBTLEPlugin;
using System.Collections.Specialized;
using System.Runtime.InteropServices.WindowsRuntime;
using TestApp.Sample;
using System.Linq;
#endif

public class ButtonHandlers : MonoBehaviour
{
    TextMeshPro feedbackText;
    public GameObject DeviceConnectButton;
    private Vector3 _lastPos = new Vector3(0.03f, 0f, 0.0001f);
    [SerializeField] private GridObjectCollection _grid;

    // A unity defined script method.  Called when the script object is first created.
    public void Start()
    {
        _grid = GameObject.FindObjectOfType<GridObjectCollection>();
    }

    void Update ()
    {
        while(BLEManager.Instance.getFeedback().Count > 0)
        {
            _ShowFeedback(BLEManager.Instance.getFeedback()[0]);
            BLEManager.Instance.getFeedback().RemoveAt(0);
        }
    }

    public void OnEnumerateClicked()
    {
        BLEManager.Instance.Enumerate();
    }

    public void OnConnectServicesClicked()
    {
        var theCachedDevices = BLEManager.Instance.ListSampleDevices();
        Debug.Log("Connect clicked");
        Debug.Log("[DEBUG] EE the length of the list is " + theCachedDevices.Count);
        foreach(var dev in theCachedDevices) {
            Debug.Log("[DEBUG] EE the cachedDevices are " + dev.Name);
        }
        SampleDevice sampDev = theCachedDevices.SingleOrDefault(device => device.Name.Contains("DISTO"));
        Debug.Log("Found the needed device " + sampDev);
        if (sampDev != null) {
            bool connected = sampDev.Connect();
            Debug.Log("[DEBUG] EE connected is " + connected);
        }
        _ShowFeedback("Connect to DISTO clicked");
    }

    public void OOnDoSomethingClicked()
    {
        #if UNITY_WSA && !UNITY_EDITOR
        // Call a method on the device based on the manufacturer specifications.
        // if (theSelectedDevice != null)
        //    theSelectedDevice.DoSomething();
        #endif
    }
    

    void _ShowFeedback(string msg)
    {
        var ins = Instantiate(DeviceConnectButton, GameObject.Find("Grid").transform, true);
        _grid.UpdateCollection();
        ins.transform.position = new Vector3(_lastPos.x, _lastPos.y - 0.01f, _lastPos.z);
        _lastPos.y -= 0.01f;
        ins.name = msg;
        feedbackText = ins.GetComponentInChildren<TextMeshPro>();
        feedbackText.SetText("Connect to " + msg);
    }


    public void ShowFeedback(string msg)
    {
        BLEManager.Instance.getFeedback().Add(msg);
    }
}