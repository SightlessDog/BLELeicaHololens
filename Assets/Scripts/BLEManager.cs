using System;
using System.Collections.Generic;
using UnityEngine;

public class BLEManager : Singleton<BLEManager>
{
    // The selected device we are connecting with.
    private string selectedDeviceId;
    private string selectedServiceId;
    private string characteristicId;
    private bool subscribed;
    private bool customLeicaValue;
    Dictionary<string, Dictionary<string, string>>
        deviceServices = new Dictionary<string, Dictionary<string, string>>();
    Dictionary<string, Dictionary<string, string>> serviceCharacteristics =
        new Dictionary<string, Dictionary<string, string>>();
    private GameObject commandsList; 

    public string GetDeviceId()
    {
        return selectedDeviceId;
    }

    public void SetDeviceId(string id)
    {
        selectedDeviceId = id;
    }

    public string GetServiceId()
    {
    return selectedServiceId; 
    }

    public void SetServiceId(string id)
    {
        selectedServiceId = id;
    }

    public void setSubscribed(bool subsc)
    {
        subscribed = subsc;
    }

    public bool getSubscribed()
    {
        return subscribed;
    }

    public void setCustomLeicaValue(bool custom)
    {
        customLeicaValue = custom; 
    }

    public bool getCustomLeicaValue()
    {
        return customLeicaValue;
    }

    public void HandleComingServiceData(string uuid)
    {
        if (!deviceServices.ContainsKey(uuid))
        {
            deviceServices[uuid] = new Dictionary<string, string>()
            {
                { "name", UuidConverter.ConvertUuidToName(Guid.Parse(uuid)) }
            };
        }
    }

    public Dictionary<string, Dictionary<string, string>> getServiceList()
    {
        return deviceServices;
    }

    public void HandleComingCharacteristicsData(string uuid)
    {
        if (!serviceCharacteristics.ContainsKey(uuid))
        {
            serviceCharacteristics[uuid] = new Dictionary<string, string>()
            {
                { "name", UuidConverter.ConvertUuidToName(Guid.Parse(uuid)) }
            };
        }
    }
    
    public Dictionary<string, Dictionary<string, string>> getCharacteristicsList()
    {
        return serviceCharacteristics;
    }

    public void SetCommandsList(GameObject gameObject)
    {
        commandsList = gameObject;
    }

    public GameObject getCommandsList()
    {
        return commandsList;
    }

    public void setCharacteristicId(string id)
    {
        characteristicId = id;
    }

    public string getCharacteristicId()
    {
        return characteristicId;
    }
} 