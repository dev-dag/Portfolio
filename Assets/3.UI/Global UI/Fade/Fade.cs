using System;
using UnityEngine;
using UnityEngine.UI;

public class Fade : View
{
    [SerializeField] private Image fadeImage;

    private Awaitable fadeAwaiter = null;

    public override void Init()
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        if (fadeAwaiter != null)
        {
            fadeAwaiter.Cancel();
            fadeAwaiter = null;
        }

        base.Init();
    }

    /// <summary>
    /// 화면을 페이드 인 하는 함수
    /// </summary>
    /// <param name="duration">페이드 인에 걸리는 시간</param>
    /// <param name="callback">페이드 인이 끝난 후 호출 될 콜백</param>
    public void FadeIn(float duration = 1f, Action callback = null)
    {
        if (fadeAwaiter != null)
        {
            fadeAwaiter.Cancel();
        }

        fadeAwaiter = StartFade(1f, 0f, duration, callback);
    }

    /// <summary>
    /// 화면을 페이드 아웃하는 함수
    /// </summary>
    /// <param name="duration">페이드 아웃에 걸리는 시간</param>
    /// <param name="callback">페이드 아웃이 끝난 후 호출 될 콜백</param>
    public void FadeOut(float duration = 1f, Action callback = null)
    {
        if (fadeAwaiter != null)
        {
            fadeAwaiter.Cancel();
        }

        fadeAwaiter = StartFade(0f, 1f, duration, callback);
    }

    /// <summary>
    /// 프레임 단위로 페이딩을 수행하는 함수.
    /// </summary>
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
