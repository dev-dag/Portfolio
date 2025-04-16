using UnityEngine;

public class Skill : PoolingObject, ICombatAnimatorEventListener
{
    public bool IsParryed { get; protected set; }

    [SerializeField] protected Animator effectAnimator;

    protected ProxyCollider proxyCollider;

    [SerializeField] protected SkillData data;
    
    protected int layer = 0;
    protected BaseObject caller;

    protected bool isHitable = false;

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

        IsParryed = false;

        (this as ICombatAnimatorEventListener).StopHit();
        proxyCollider.Return();
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
    /// 스킬 타입 반환
    /// </summary>
    public SkillData.SkillType GetSkillType()
    {
        return data.skillType;
    }

    public virtual void TryParry()
    {
        if (isHitable)
        {
            var vfx = GameManager.Instance.combatSystem.GetTakeHitVFX();
            vfx.Init(transform.position);
            vfx.Enable();

            IsParryed = true;

            Return();
        }
    }

    /// <summary>
    /// 애니메이터에서 이벤트로 호출되는 피격판정을 시작하는 함수
    /// </summary>
    void ICombatAnimatorEventListener.StartHit()
    {
        if (proxyCollider != null)
        {
            return;
        }

        isHitable = true;

        switch (data.collisionType)
        {
            case SkillData.SkillCollisionType.Box:
            {
                BoxProxyCollider boxProxyCollider = GameManager.Instance.combatSystem.GetBoxProxyCollider();
                proxyCollider = boxProxyCollider;

                boxProxyCollider.Init(this.transform.position, this.transform.rotation, data.colliderOffset, data.colliderSize, layer, OnHit, this);

                proxyCollider.Enable();

                break;
            }
            case SkillData.SkillCollisionType.Circle:
            {
                CircleProxyCollider circleProxyCollider = GameManager.Instance.combatSystem.GetCircleProxyCollider();
                proxyCollider = circleProxyCollider;

                circleProxyCollider.Init(this.transform.position, this.transform.rotation, data.colliderOffset, data.radius, layer, OnHit, this);

                proxyCollider.Enable();

                break;
            }
            default:
            {
                EDebug.LogError("스킬 데이터 오류 발생");

                break;
            }
        }
    }

    /// <summary>
    /// 애니메이터에서 이벤트로 호출되는 피격판정을 종료하는 함수
    /// </summary>
    void ICombatAnimatorEventListener.StopHit()
    {
        isHitable = false;
    }

    /// <summary>
    /// 피격 범위에 들어온 대상의 체력을 감소시키는 함수.
    /// </summary>
    /// <param name="combatInterface">체력을 감소시키는 함수를 제공하는 인터페이스</param>
    protected virtual void OnHit(Collider2D collision)
    {
        if (IsParryed || isHitable == false) // 충돌 가능 타이밍이 아닌 경우나, 패리된 스킬은 물리처리 하지 않음.
        {
            return;
        }

        BaseObject hitObject = collision.attachedRigidbody.GetComponent<BaseObject>();

        if (hitObject is ICombatable)
        {
            ICombatable combatInterface = collision.attachedRigidbody.GetComponent<BaseObject>() as ICombatable;

            combatInterface.TakeHit(data.damage, caller);
        }
        else if (hitObject is ProxyCollider) // 스킬과 충돌 체크된 경우 패리 가능한지 체크
        {
            Skill hitSkill = (hitObject as ProxyCollider).GetSkill();

            if (hitSkill.GetSkillType() == SkillData.SkillType.Parried
                && data.skillType == SkillData.SkillType.Parryable)
            {
                hitSkill.TryParry();
            }
        }
    }
}
