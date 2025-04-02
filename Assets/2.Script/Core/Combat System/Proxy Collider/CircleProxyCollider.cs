using System;
using UnityEngine;

public class CircleProxyCollider : ProxyCollider
{
    private CircleCollider2D collider;

    protected override void Awake()
    {
        base.Awake();

        collider = GetComponent<CircleCollider2D>();
    }

    public void Init(Vector2 worldPosition, float radius, Action<ICombatable> onHitCallback)
    {
        this.transform.position = worldPosition;

        collider.radius = radius;

        onHitEventHandler += onHitCallback;
    }
}