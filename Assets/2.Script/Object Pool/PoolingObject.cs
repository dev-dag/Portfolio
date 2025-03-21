using System;
using UnityEngine;

public abstract class PoolingObject : BaseObject
{
    protected ObjectPool pool;

    /// <summary>
    /// 생성 시 호출
    /// </summary>
    public virtual void SetPool(ObjectPool pool)
    {
        this.pool = pool;
    }

    /// <summary>
    /// 풀로 반환
    /// </summary>
    public virtual void Return()
    {
        Disable();

        this.pool.Return(this);
    }

    /// <summary>
    /// 사용하기 전에 수동 호출
    /// </summary>
    public virtual void Enable()
    {
        this.gameObject.SetActive(true);

        return;
    }

    /// <summary>
    /// 사용한 후 반환할 때 자동 호출
    /// </summary>
    public virtual void Disable()
    {
        if (transform.parent != pool.transform)
        {
            transform.SetParent(pool.transform);
        }

        this.gameObject.SetActive(false);

        return;
    }
}
