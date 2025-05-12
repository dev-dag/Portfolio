using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPlayer : PoolingObject
{
    public int Id { get => id; } // AudioSystem이 할당한 ID
    public bool IsPlaing { get => audioSource.isPlaying; } // 현재 재생 중인지 여부

    [SerializeField] private AudioSource audioSource; // 오디오 플레이어 소스

    private int id; // 오디오 플레이어 ID
    private Action<int> onEndCallback; // 재생이 끝났을 때 호출할 콜백
    private Awaitable checkAwaiter; // 재생 완료 체크를 위한 Awaitable

    private bool isInit = false;

    public override void Return()
    {
        base.Return();

        id = -1;
        isInit = false;
        onEndCallback = null;

        if (checkAwaiter != null)
        {
            checkAwaiter.Cancel();
            checkAwaiter = null;
        }
    }

    /// <summary>
    /// 오디오 플레이어를 초기화하는 함수
    /// </summary>
    /// <param name="id">AudioSystem에 의해 관리될 ID.</param>
    /// <param name="onEndCallback">재생이 끝났을 때 호출될 콜백</param>
    public void Init(int id, Action<int> onEndCallback)
    {
        this.onEndCallback = onEndCallback; // 콜백 초기화
        audioSource.loop = false;
        audioSource.playOnAwake = false; // 자동 재생 방지
        Enable();

        isInit = true;
    }

    /// <summary>
    /// 오디오를 루프로 설정하는 함수
    /// </summary>
    public void SetLoop(bool isLoop = true)
    {
        if (isInit == false)
        {
            return;
        }

        audioSource.loop = isLoop; // 루프 설정
    }

    /// <summary>
    /// 현재 재생 혹은 대기중인 클립의 참조 반환
    /// </summary>
    /// <returns></returns>
    public AudioClip GetCurrentClip()
    {
        if (isInit == false)
        {
            return null;
        }

        return audioSource.clip;
    }

    /// <summary>
    /// 오디오를 재생하는 함수
    /// </summary>
    public void Play(AudioClip clip)
    {
        if (isInit == false)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop(); // 정지
        }

        audioSource.clip = clip;
        audioSource.Play();

        checkAwaiter = CompleteCheck(); // 재생 완료 체크 시작
    }

    /// <summary>
    /// 일시 정지하는 함수
    /// </summary>
    public void Pause()
    {
        if (isInit == false)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Pause(); // 일시 정지
        }
    }

    /// <summary>
    /// 일시 정지 해제하는 함수
    /// </summary>
    public void Resume()
    {
        if (isInit == false)
        {
            return;
        }

        audioSource.UnPause(); // 일시 정지 해제
    }

    /// <summary>
    /// 오디오를 정지하는 함수
    /// </summary>
    public void Stop()
    {
        if (isInit == false)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop(); // 정지
        }
    }

    /// <summary>
    /// 볼륨 설정하는 함수
    /// </summary>
    public void SetVolume(float volume)
    {
        if (isInit == false)
        {
            return;
        }

        audioSource.volume = volume; // 볼륨 설정
    }

    private async Awaitable CompleteCheck()
    {
        while (audioSource.isPlaying)
        {
            await Awaitable.WaitForSecondsAsync(1f);
        }

        onEndCallback?.Invoke(id); // 재생이 끝났을 때 콜백 호출
    }
}
