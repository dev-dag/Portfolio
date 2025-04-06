using System;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    public void Init(Vector2 worldPosition, Vector2 offset, Vector2 size, int layer, Action<ICombatable> onHitCallback)
    {
        this.transform.position = worldPosition;
        this.gameObject.layer = layer;

        collider.size = size;
        collider.offset = offset;

        onHitEventHandler += onHitCallback;
    }

    public override void Return()
    {
        base.Return();
    }
}