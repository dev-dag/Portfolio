using System;
using UnityEngine;

/// <summary>
/// Skill에서 사용하기 위한 대리 충돌체
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