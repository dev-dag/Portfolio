using System;
using UnityEngine;

/// <summary>
/// Skill에서 사용하기 위한 대리 충돌체
/// </summary>
public class CircleProxyCollider : ProxyCollider
{
    private CircleCollider2D collider;

    protected override void Awake()
    {
        base.Awake();

        collider = GetComponent<CircleCollider2D>();
    }

    public void Init(Vector2 position, Quaternion rotation, Vector2 offset, float radius, int layer, Action<Collider2D> onHitCallback, SkillAction owner)
    {
        base.ownerSkill = owner;

        this.transform.position = position;
        this.transform.rotation = rotation;
        this.gameObject.layer = layer;

        collider.radius = radius;
        collider.offset = offset;

        onHitEventHandler += onHitCallback;
    }

    public override void Return()
    {
        this.transform.position = Vector3.zero;
        this.gameObject.layer = 0;

        collider.radius = 1f;
        collider.offset = Vector3.zero;

        base.Return();
    }
}