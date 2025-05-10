using UnityEngine;

public abstract class LevelControl : MonoBehaviour
{
    [SerializeField] protected Vector2 startPoint;
    [SerializeField] protected AudioClip BGM_Clip;

    public static LevelControl Current { get; protected set; }

    protected void PlayBGM()
    {
        if (BGM_Clip != null)
        {
            GameManager.Instance.audioSystem.GetBGM_Player().Play(BGM_Clip);
        }
    }

    private void Awake()
    {
        Current = this;
        Player.Current.transform.position = startPoint;
        Player.Current.AttachCamera();
    }

    private void Start()
    {
        GameManager.Instance.gameUI.ShowUI_ForCinematic(true);
        PlayBGM();
    }
}
