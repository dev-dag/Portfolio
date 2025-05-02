using System;
using UnityEngine;

/// <summary>
/// Skill에서 사용하기 위한 대리 충돌체
/// </summary>
public class BoxProxyCollider : ProxyCollider
{
    private BoxCollider2D boxCollider;

    protected override void Awake()
    {
        base.Awake();

        boxCollider = GetComponent<BoxCollider2D>();
    }

#if DEBUG

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (this.transform.rotation.eulerAngles.y == 0f)
        {
            Gizmos.DrawWireCube((Vector2)transform.position + boxCollider.offset * new Vector2(1f, 1f), boxCollider.size);
        }
        else if (this.transform.rotation.eulerAngles.y == 180f)
        {
            Gizmos.DrawWireCube((Vector2)transform.position + boxCollider.offset * new Vector2(-1f, 1f), boxCollider.size);
        }
    }

#endif

    public void Init(Vector2 position, Quaternion rotation, Vector2 offset, Vector2 size, int layer, Action<Collider2D> onHitCallback, SkillAction owner)
    {
        base.ownerSkill = owner;

        this.transform.position = position;
        this.transform.rotation = rotation;
        this.gameObject.layer = layer;

        boxCollider.size = size;
        boxCollider.offset = offset;

        onHitEventHandler += onHitCallback;
    }

    public override void Return()
    {
        base.Return();
    }
}