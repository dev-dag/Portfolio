using System;
using UnityEngine;

/// <summary>
/// Skill���� ����ϱ� ���� �븮 �浹ü
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class ProxyCollider : PoolingObject
{
    public event Action<ICombatable, Rigidbody2D> onHitEventHandler;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody.GetComponentInChildren<BaseObject>() is ICombatable)
        {
            ICombatable combatInterface = collision.attachedRigidbody.GetComponentInChildren<BaseObject>() as ICombatable;

            onHitEventHandler?.Invoke(combatInterface, collision.attachedRigidbody);
        }
    }

    public override void Return()
    {
        base.Return();

        onHitEventHandler = null;
    }
}