using System;
using UnityEngine;

/// <summary>
/// Skill에서 사용하기 위한 대리 충돌체
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class ProxyCollider : PoolingObject
{
    public event Action<ICombatable> onHitEventHandler;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody.GetComponentInChildren<BaseObject>() is ICombatable)
        {
            ICombatable combatInterface = collision.attachedRigidbody.GetComponentInChildren<BaseObject>() as ICombatable;

            onHitEventHandler?.Invoke(combatInterface);
        }
    }

    public override void Return()
    {
        base.Return();

        onHitEventHandler = null;
    }
}