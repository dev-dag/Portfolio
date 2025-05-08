using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : BaseObject
{
    [SerializeField] private ObjectPool audioPlayerPool; // 오디오 플레이어 풀

    private AudioPlayer BGM_Player; // BGM 플레이어 캐시
    private Dictionary<int, AudioPlayer> SFX_Player; // 재생중인 SFX 플레이어

    public void Init()
    {
        // BGM 초기화
        if (BGM_Player == null)
        {
            BGM_Player = audioPlayerPool.Burrow<AudioPlayer>();
            int playerID = BGM_Player.GetInstanceID();
            BGM_Player.SetLoop(); // BGM 루프 설정
            BGM_Player.Init(playerID, null); // BGM 플레이어 초기화    
        }

        SFX_Player = new Dictionary<int, AudioPlayer>();
    }

    /// <summary>
    /// BGM 플레이어를 반환하는 함수
    /// </summary>
    public AudioPlayer GetBGM_Player()
    {
        return BGM_Player;
    }

    /// <summary>
    /// SFX 플레이어를 반환하는 함수
    /// </summary>
    public AudioPlayer GetSFX_Player()
    {
        AudioPlayer player = audioPlayerPool.Burrow<AudioPlayer>();
        int playerID = player.GetInstanceID();
        player.Init(playerID, OnSFXEnd); // SFX 플레이어 초기화
        SFX_Player.Add(playerID, player); // SFX 플레이어 추가

        return player;
    }

    /// <summary>
    /// SFX를 재생하는 함수
    /// </summary>
    public void PlaySFX(AudioClip audioClip)
    {
        var sfx = GameManager.Instance.audioSystem.GetSFX_Player(); // SFX 재생
        sfx.Enable();
        sfx.Play(audioClip);
    }

    /// <summary>
    /// SFX 플레이어가 재생이 끝났을 때 호출되는 콜백 함수
    /// </summary>
    private void OnSFXEnd(int playerID)
    {
        if (SFX_Player.ContainsKey(playerID))
        {
            SFX_Player[playerID].Return(); // SFX 플레이어 반환
            SFX_Player.Remove(playerID); // SFX 플레이어 제거
        }
    }
}
