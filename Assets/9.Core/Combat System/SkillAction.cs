﻿using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class SkillAction : PoolingObject, ICombatAnimatorEventListener
{
    public struct Option
    {
        public bool applyFollow;
        public Transform followTarget;

        public Option(Transform followTarget) { applyFollow = true; this.followTarget = followTarget; }
    }

    public bool IsParryed { get; protected set; }

    [SerializeField] protected Animator effectAnimator;
    protected SkillData data;

    protected ProxyCollider proxyCollider;
    protected BaseObject caller;
    protected int layer = 0;
    protected bool isHitable = false;
    protected int weaponDamage = 0;
    protected Option option;

    public void Init(int weaponDamage, Vector2 position, Quaternion rotation, int layer, SkillData data, BaseObject caller, Option option)
    {
        this.data = data;
        this.transform.rotation = rotation;
        this.layer = layer;
        this.caller = caller;
        this.weaponDamage = weaponDamage;
        this.option = option;

        if (option.applyFollow)
        {
            this.transform.position = option.followTarget.position;
        }
        else
        {
            this.transform.position = position;
        }

            effectAnimator.transform.localPosition = data.VFX_Offset;
        effectAnimator.runtimeAnimatorController = data.animationController;

        this.gameObject.SetActive(false);
    }

    public override void Enable()
    {
        base.Enable();

        if (data.useLifeTime)
        {
            Invoke("Return", data.lifeTime);
        }
    }

    public override void Return()
    {
        base.Return();

        IsParryed = false;

        (this as ICombatAnimatorEventListener).StopHit();

        CancelInvoke();

        if (data.collisionType != SkillData.SkillCollisionType.None)
        {
            proxyCollider.Return();
            proxyCollider = null;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (IsReturned)
        {
            return;
        }

        if (option.applyFollow)
        {
            transform.position = option.followTarget.position;

            if (isHitable && data.collisionType != SkillData.SkillCollisionType.None)
            {
                proxyCollider.transform.position = transform.position;
            }
        }
    }

    /// <summary>
    /// 스킬 타입 반환
    /// </summary>
    public SkillData.ParryType GetSkillType()
    {
        return data.parryType;
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
        isHitable = true;

        if (proxyCollider != null)
        {
            proxyCollider.Enable();
            return;
        }

        switch (data.collisionType)
        {
            case SkillData.SkillCollisionType.None:
            {
                break;
            }
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

        if (data.collisionType != SkillData.SkillCollisionType.None)
        {
            proxyCollider.Disable();
        }
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

        BaseObject hitObject = collision.attachedRigidbody?.GetComponent<BaseObject>();

        if (hitObject is ICombatable)
        {
            ICombatable combatInterface = collision.attachedRigidbody.GetComponent<BaseObject>() as ICombatable;

            combatInterface.TakeHit(weaponDamage + data.additionalDamage, caller);
        }
        else if (hitObject is ProxyCollider) // 스킬과 충돌 체크된 경우 패리 가능한지 체크
        {
            SkillAction hitSkill = (hitObject as ProxyCollider).GetSkill();

            if (hitSkill.GetSkillType() == SkillData.ParryType.Parried
                && data.parryType == SkillData.ParryType.Parryable)
            {
                hitSkill.TryParry();
            }
        }
    }
}
