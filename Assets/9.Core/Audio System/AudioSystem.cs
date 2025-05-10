using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : BaseObject
{
    public enum AudioType
    {
        BGM,
        SFX,
        UI_SFX
    }

    public bool IsInit { get; private set; } = false; // 초기화 여부

    [SerializeField] private ObjectPool audioPlayerPool; // 오디오 플레이어 풀

    private AudioPlayer BGM_Player; // BGM 플레이어 캐시
    private Dictionary<int, AudioPlayer> SFX_Player; // 재생중인 SFX 플레이어
    private Dictionary<int, AudioPlayer> UI_SFX_Player; // UI SFX 플레이어

    public void Init()
    {
        // BGM 초기화
        if (BGM_Player == null)
        {
            BGM_Player = audioPlayerPool.Burrow<AudioPlayer>();
            int playerID = BGM_Player.GetInstanceID();
            BGM_Player.gameObject.name = "BGM Player";
            BGM_Player.Init(playerID, null); // BGM 플레이어 초기화    
            BGM_Player.SetLoop(); // BGM 루프 설정
        }

        SFX_Player = new Dictionary<int, AudioPlayer>();
        UI_SFX_Player = new Dictionary<int, AudioPlayer>();

        IsInit = true;
    }

    /// <summary>
    /// /// <summary>
    /// 오디오 플레이어를 반환하는 함수
    /// </summary>
    /// <param name="audioType">오디오 분류. BGM은 반환하지 않음.</param>
    public AudioPlayer GetAudioPlayer(AudioType audioType)
    {
        switch (audioType)
        {
            case AudioType.SFX:
            {
                AudioPlayer player = audioPlayerPool.Burrow<AudioPlayer>();
                int playerID = player.GetInstanceID();
                player.Init(playerID, OnSFXEnd); // SFX 플레이어 초기화
                player.Enable(); // SFX 플레이어 활성화
                SFX_Player.Add(playerID, player); // SFX 플레이어 추가

                return player;
            }
            case AudioType.UI_SFX:
            {
                AudioPlayer player = audioPlayerPool.Burrow<AudioPlayer>();
                int playerID = player.GetInstanceID();
                player.Init(playerID, OnSFXEnd); // SFX 플레이어 초기화
                player.Enable(); // SFX 플레이어 활성화
                UI_SFX_Player.Add(playerID, player); // SFX 플레이어 추가

                return player;
            }
        }

        return null; // BGM은 반환하지 않음
    }

    /// <summary>
    /// 재생이 끝난 후 자동으로 반환되지 않는 오디오 플레이어를 반환하는 함수. BGM이 필요한 경우 "GetBGM_Player()" 사용
    /// </summary>
    /// <param name="audioType">오디오 분류. BGM은 반환하지 않음.</param>
    public AudioPlayer GetUnManagedAudioPlayer(AudioType audioType)
    {
        switch (audioType)
        {
            case AudioType.SFX:
            {
                AudioPlayer player = audioPlayerPool.Burrow<AudioPlayer>();
                int playerID = player.GetInstanceID();
                player.Init(playerID, null); // SFX 플레이어 초기화
                player.Enable(); // SFX 플레이어 활성화
                SFX_Player.Add(playerID, player); // SFX 플레이어 추가

                return player;
            }
            case AudioType.UI_SFX:
            {
                AudioPlayer player = audioPlayerPool.Burrow<AudioPlayer>();
                int playerID = player.GetInstanceID();
                player.Init(playerID, null); // SFX 플레이어 초기화
                player.Enable(); // SFX 플레이어 활성화
                UI_SFX_Player.Add(playerID, player); // SFX 플레이어 추가

                return player;
            }
        }

        return null; // BGM은 반환하지 않음
    }

    /// <summary>
    /// BGM 플레이어를 반환하는 함수
    /// </summary>
    public AudioPlayer GetBGM_Player()
    {
        return BGM_Player;
    }

    /// <summary>
    /// SFX를 재생하는 함수
    /// </summary>
    public void PlaySFX(AudioType audioType, AudioClip audioClip)
    {
        var audioPlayer = GetAudioPlayer(audioType); // SFX 플레이어 가져오기
        audioPlayer.Play(audioClip); // SFX 재생
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
        else if (UI_SFX_Player.ContainsKey(playerID))
        {
            UI_SFX_Player[playerID].Return(); // SFX 플레이어 반환
            UI_SFX_Player.Remove(playerID); // SFX 플레이어 제거
        }
    }
}
