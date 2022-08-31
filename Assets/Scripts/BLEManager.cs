public class BLEManager : Singleton<BLEManager>
{
    // The selected device we are connecting with.
    private string selectedDeviceId;
    private string selectedServiceId;
    private bool subscribed;

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
}