using System;
using UnityEngine;

/// <summary>
/// Skill���� ����ϱ� ���� �븮 �浹ü
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class ProxyCollider : PoolingObject
{
    public event Action<Collider2D> onHitEventHandler;
    protected Skill ownerSkill;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onHitEventHandler?.Invoke(collision);
    }

    public override void Return()
    {
        base.Return();

        onHitEventHandler = null;
        ownerSkill = null;
    }

    public Skill GetSkill()
    {
        return ownerSkill;
    }
}