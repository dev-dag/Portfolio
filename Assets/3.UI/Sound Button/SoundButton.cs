using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI 버튼에 소리 효과를 추가하는 컴포넌트.
/// </summary>
[RequireComponent(typeof(Button))]
public class SoundButton : BaseObject, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] private AudioClip overAudioClip;
    [SerializeField] private AudioClip clickAudioClip;

    // 클릭 되었을 때 SFX 재생
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.audioSystem.IsInit)
        {
            GameManager.Instance.audioSystem.PlaySFX(AudioSystem.AudioType.UI_SFX, clickAudioClip);
        }
    }

    // 오버 되었을 때 SFX 재생
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance.audioSystem.IsInit)
        {
            GameManager.Instance.audioSystem.PlaySFX(AudioSystem.AudioType.UI_SFX, overAudioClip);
        }
    }
}
