using UnityEngine;

public class Skill : PoolingObject, ICombatAnimatorEventListener
{
    [SerializeField] protected Animator effectAnimator;

    protected ProxyCollider proxyCollider;

    [SerializeField] protected SkillData data;
    
    protected int layer = 0;
    protected BaseObject caller;

    public void Init(Vector2 position, Quaternion rotation, int layer, SkillData data, BaseObject caller)
    {
        this.data = data;
        this.transform.position = position;
        this.transform.rotation = rotation;
        this.layer = layer;
        this.caller = caller;

        effectAnimator.transform.localPosition = data.VFX_Offset;
        effectAnimator.runtimeAnimatorController = data.animationController;

        this.gameObject.SetActive(false);
    }

    public override void Enable()
    {
        base.Enable();

        Invoke("Return", data.lifeTime);
    }

    public override void Return()
    {
        base.Return();

        (this as ICombatAnimatorEventListener).StopHit();
        proxyCollider = null;
    }

    protected override void Update()
    {
        base.Update();
    }

#if DEBUG

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (data.collisionType == SkillData.SkillCollisionType.Box)
        {
            Gizmos.DrawWireCube((Vector2)transform.position + data.colliderOffset, data.colliderSize);
        }
        else if (data.collisionType == SkillData.SkillCollisionType.Circle)
        {
            Gizmos.DrawWireSphere((Vector2)transform.position + data.colliderOffset, data.radius);
        }
    }

#endif

    /// <summary>
    /// �ִϸ����Ϳ��� �̺�Ʈ�� ȣ��Ǵ� �ǰ������� �����ϴ� �Լ�
    /// </summary>
    void ICombatAnimatorEventListener.StartHit()
    {
        if (proxyCollider != null)
        {
            return;
        }

        switch (data.collisionType)
        {
            case SkillData.SkillCollisionType.Box:
            {
                BoxProxyCollider boxProxyCollider = GameManager.Instance.combatSystem.GetBoxProxyCollider();
                proxyCollider = boxProxyCollider;

                boxProxyCollider.Init(this.transform.position, this.transform.rotation, data.colliderOffset, data.colliderSize, layer, OnHit);

                proxyCollider.Enable();

                break;
            }
            case SkillData.SkillCollisionType.Circle:
            {
                CircleProxyCollider circleProxyCollider = GameManager.Instance.combatSystem.GetCircleProxyCollider();
                proxyCollider = circleProxyCollider;

                circleProxyCollider.Init(this.transform.position, this.transform.rotation, data.colliderOffset, data.radius, layer, OnHit);

                proxyCollider.Enable();

                break;
            }
            default:
            {
                Debug.LogError("��ų ������ ���� �߻�");

                break;
            }
        }
    }

    /// <summary>
    /// �ִϸ����Ϳ��� �̺�Ʈ�� ȣ��Ǵ� �ǰ������� �����ϴ� �Լ�
    /// </summary>
    void ICombatAnimatorEventListener.StopHit()
    {
        proxyCollider?.Return();
        proxyCollider = null;
    }

    /// <summary>
    /// �ǰ� ������ ���� ����� ü���� ���ҽ�Ű�� �Լ�.
    /// </summary>
    /// <param name="combatInterface">ü���� ���ҽ�Ű�� �Լ��� �����ϴ� �������̽�</param>
    protected virtual void OnHit(Collider2D collision)
    {
        if (collision.attachedRigidbody.GetComponentInChildren<BaseObject>() is ICombatable)
        {
            ICombatable combatInterface = collision.attachedRigidbody.GetComponentInChildren<BaseObject>() as ICombatable;

            combatInterface.TakeHit(data.damage, caller);
        }
    }
}
