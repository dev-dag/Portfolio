using UnityEngine;

public class PoolingView : PoolingObject
{
    public bool IsInit { get; private set; } = false;

    public virtual void Init()
    {
        IsInit = true;
    }
}
