using System;
using UnityEngine;

/// <summary>
/// Skill���� ����ϱ� ���� �븮 �浹ü
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class ProxyCollider : PoolingObject
{
    public event Action<ICombatable> onHitEventHandler;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ICombatable combatInterface = collision.GetComponentInChildren<ICombatable>();

        if (combatInterface != null)
        {
            onHitEventHandler?.Invoke(combatInterface);
        }
    }

    public override void Return()
    {
        base.Return();

        onHitEventHandler = null;
    }
}