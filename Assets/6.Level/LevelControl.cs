using UnityEngine;

public abstract class LevelControl : BaseObject
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

    protected override void Awake()
    {
        base.Awake();

        Current = this;
        Player.Current.transform.position = startPoint;
        Player.Current.AttachCamera();
    }

    protected override void Start()
    {
        base.Start();

        GameManager.Instance.uiManager.ShowUI_ForCinematic(true);
        PlayBGM();
    }
}
