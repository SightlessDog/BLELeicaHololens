using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class Vec : MonoBehaviour
{
    [SerializeField] private GameObject dialogPrefab;
    private GameObject measurment;

    public void OnPress(GameObject vec)
    {
        measurment = vec;
        Dialog myDialog = Dialog.Open(dialogPrefab, DialogButtonType.Yes | DialogButtonType.No, "Delete",
            "Do you really want to delete this measurement?", true);
        if (myDialog != null)
        {
            myDialog.OnClosed += OnClosedDialogEvent;
        }
    }

    private void OnClosedDialogEvent(DialogResult obj)
    {
        if (obj.Result == DialogButtonType.Yes)
        {
            ShapeBuilder.Instance.RemoveFromMeasurements(measurment.name);
            Destroy(GameObject.Find(measurment.name));
            NotificationManager.Instance.SetNewNotification("Measurement deleted");
        }
    }
}