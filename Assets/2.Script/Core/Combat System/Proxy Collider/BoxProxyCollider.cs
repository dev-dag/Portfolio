using System;
using UnityEngine;

/// <summary>
/// Skill���� ����ϱ� ���� �븮 �浹ü
/// </summary>
public class BoxProxyCollider : ProxyCollider
{
    private BoxCollider2D collider;

    protected override void Awake()
    {
        base.Awake();

        collider = GetComponent<BoxCollider2D>();
    }

    public void Init(Vector2 worldPosition, Vector2 size, Action<ICombatable> onHitCallback)
    {
        this.transform.position = worldPosition;

        collider.size = size;

        onHitEventHandler += onHitCallback;
    }
}