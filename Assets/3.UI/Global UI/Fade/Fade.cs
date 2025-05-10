using System;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    private Awaitable fadeAwaiter = null;

    private void Start()
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
    }

    public void FadeIn(float duration = 1f, Action callback = null)
    {
        if (fadeAwaiter != null)
        {
            fadeAwaiter.Cancel();
        }

        fadeAwaiter = StartFade(1f, 0f, duration, callback);
    }

    public void FadeOut(float duration = 1f, Action callback = null)
    {
        if (fadeAwaiter != null)
        {
            fadeAwaiter.Cancel();
        }

        fadeAwaiter = StartFade(0f, 1f, duration, callback);
    }

    private async Awaitable StartFade(float fromAlpha, float toAlpha, float duration, Action callback = null)
    {
        float time = 0f;

        fadeImage.color = new Color(0f, 0f, 0f, fromAlpha);

        while (time < duration)
        {
            time += Time.deltaTime;
            fadeImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(fromAlpha, toAlpha, time / duration));
            await Awaitable.NextFrameAsync();
        }

        fadeImage.color = new Color(0f, 0f, 0f, toAlpha);

        fadeAwaiter = null;

        callback?.Invoke();
    }
}
