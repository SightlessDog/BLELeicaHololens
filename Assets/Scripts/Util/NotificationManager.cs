using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationManager : Singleton<NotificationManager>
{
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float fadeTime;
    private IEnumerator notificationCoroutine;

    private void Awake()
    {
        ButtonHandlers.onStateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        ButtonHandlers.onStateChanged -= OnStateChanged;
    }

    public void SetNewNotification(string text, bool fade = true)
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        notificationCoroutine = FadeOutNotification(text, fade);
        StartCoroutine(notificationCoroutine);
    }

    private IEnumerator FadeOutNotification(string text, bool fade)
    {
        notificationText.text = text;
        if (fade)
        {
            float t = 0;
            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime;
                notificationText.color = new Color(notificationText.color.r, notificationText.color.g,
                    notificationText.color.b, Mathf.Lerp(1f, 0f, t / fadeTime));
                yield return null;
            }
        }
        else
        {
            notificationText.color = new Color(notificationText.color.r, notificationText.color.g,
                notificationText.color.b, 1f);
        }
    }


    private void OnStateChanged(State state)
    {
        if (state == State.STARTED)
        {
            SetNewNotification(
                "Hello, To connect your bluetooth device you have to click first on enumerate and then click and your device",
                false);
            return;
        }

        if (state == State.ENUMERATED)
        {
            SetNewNotification(
                "Well done, now you can click on connect button", false);
            return;
        }
        if (state == State.CONNECTED)
        {
            SetNewNotification(
                "You are now successfully connected to "  +  BLEManager.Instance.GetDeviceName() + ", feel free to choose a service");
            return;
        }
        
        if (state == State.SERVICESELECTED)
        {
            SetNewNotification(
                "Don't forget to click on characteristics button to show the possibilities available to you");
        }
    }
}