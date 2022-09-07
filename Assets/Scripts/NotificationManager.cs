using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationManager : Singleton<NotificationManager>
{
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float fadeTime;
    private IEnumerator notificationCoroutine;

    public void SetNewNotification(string text)
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        notificationCoroutine = FadeOutNotification(text);
        StartCoroutine(notificationCoroutine);
    }

    private IEnumerator FadeOutNotification(string text)
    {
        notificationText.text = text;
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            notificationText.color = new Color(notificationText.color.r, notificationText.color.g,
                notificationText.color.b, Mathf.Lerp(1f, 0f, t / fadeTime));
            yield return null;
        }
    }
}