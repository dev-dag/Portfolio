using UnityEngine;

public abstract class LevelControl : BaseObject
{
    [SerializeField] protected Vector2 startPoint;

    public static LevelControl Current { get; protected set; }

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
    }
}
