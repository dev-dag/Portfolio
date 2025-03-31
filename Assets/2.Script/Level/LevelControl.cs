using UnityEngine;

public abstract class LevelControl : BaseObject
{
    [SerializeField] protected Vector2 startPoint;

    public static LevelControl Current { get; protected set; }

    protected override void Start()
    {
        base.Start();

        Current = this;
    }
}
