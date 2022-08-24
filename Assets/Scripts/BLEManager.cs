using System;
using System.Collections.Generic;
using TestApp.Sample;
using UnityEngine;
#if UNITY_WSA && !UNITY_EDITOR
using UnityUWPBTLEPlugin;
using System.Collections.Specialized;
using System.Runtime.InteropServices.WindowsRuntime;
using TestApp.Sample;
using System.Linq;
#endif

public class BLEManager : Singleton<BLEManager>
{
#if UNITY_WSA && !UNITY_EDITOR
    // The BTLE helper class
    private BluetoothLEHelper ble;
#endif

    // The selected device we are connecting with.
    private SampleDevice theSelectedDevice;
    // The cached list of BTLE devices we have seen
    private List<SampleDevice> theCachedDevices = new List<SampleDevice>();
    private List<string> feedbackMsgs = new List<string>();

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("I am awake");
    }

    /// A unity defined script method, called every tick to allow the script to perform some actions.
    public void Update()
    {
        
#if UNITY_WSA && !UNITY_EDITOR
        // If we don't have a BTLE library connection yet we need to create one
        if (ble != null)
        {
            // If the devices changed flag has been set we need to process any additions / removals 
            if (ble.DevicesChanged)
            {
                // Process the list of new devices.
                var newDeviceList = ble.BluetoothLeDevicesAdded;
                foreach (var theNewDevice in newDeviceList)
                {
                    Debug.Log("[DEBUG] EE the devices found " + theNewDevice.DeviceInfo.Id + " " + theNewDevice.Name);
                    if (theNewDevice.Name.Contains("DISTO")) {
                            Debug.Log("[DEBUG] EE trying to connect to that device " + theNewDevice);
                            SampleDevice sampDev = new SampleDevice(theNewDevice);
                            bool conn1 = sampDev.Connect();
                            Debug.Log("[DEBUG] EE after trying to connect to that device conn1 var is " + conn1);
                    }
                    // First see if we already have it in the cache
                    var item = theCachedDevices.SingleOrDefault(r => r.DeviceInfo.Id == theNewDevice.DeviceInfo.Id);
                    if (item == null)
                    {
                        // Create the wrapper for the BTLE object
                        SampleDevice newSampleDevice = new SampleDevice(theNewDevice);
                        // Add it to our cache of devices
                        Debug.Log("[DEBUG] EE adding the device to cachedDevices " + newSampleDevice.DeviceInfo.Id 
                            + " " + newSampleDevice.Name);
                        theCachedDevices.Add(newSampleDevice);
                        feedbackMsgs.Add(theNewDevice.Name);
                    }
                    else
                    {
                        feedbackMsgs.Add(theNewDevice.Name);
                    }
                }
            }
        }
#endif
    }

    public List<SampleDevice> ListSampleDevices()
    {
        return theCachedDevices;
    }

    public List<string> getFeedback()
    {
        return feedbackMsgs;
    }

    public void Enumerate()
    {
#if UNITY_WSA && !UNITY_EDITOR
        if (ble == null)
        {
            ble = BluetoothLEHelper.Instance;
        }
        ble.StartEnumeration();
        feedbackMsgs.Add("Enumerate clicked");
#endif
    }
}