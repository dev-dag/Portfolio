using System;
using UnityEngine;

/// <summary>
/// Skill���� ����ϱ� ���� �븮 �浹ü
/// </summary>
public class CircleProxyCollider : ProxyCollider
{
    private CircleCollider2D circleCollider;

    protected override void Awake()
    {
        base.Awake();

        circleCollider = GetComponent<CircleCollider2D>();
    }

#if DEBUG

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (this.transform.rotation.eulerAngles.y == 0f)
        {
            Gizmos.DrawWireSphere((Vector2)transform.position + circleCollider.offset * new Vector2(1f, 1f), circleCollider.radius);
        }
        else if (this.transform.rotation.eulerAngles.y == 180f)
        {
            Gizmos.DrawWireSphere((Vector2)transform.position + circleCollider.offset * new Vector2(-1f, 1f), circleCollider.radius);
        }
    }

#endif

    public void Init(Vector2 position, Quaternion rotation, Vector2 offset, float radius, int layer, Action<Collider2D> onHitCallback, SkillAction owner)
    {
        base.ownerSkill = owner;

        this.transform.position = position;
        this.transform.rotation = rotation;
        this.gameObject.layer = layer;

        circleCollider.radius = radius;
        circleCollider.offset = offset;

        onHitEventHandler += onHitCallback;
    }

    public override void Return()
    {
        this.transform.position = Vector3.zero;
        this.gameObject.layer = 0;

        circleCollider.radius = 1f;
        circleCollider.offset = Vector3.zero;

        base.Return();
    }
}