using System;
using UnityEngine;

public class CombatSystem : BaseObject
{
    public enum EntityType
    {
        Player,
        Enemy
    }

    public event Action OnQuickSlotChanged;

    private ItemContainer quickItemContainer_1;
    public ItemContainer QuickItemContainer_1
    {
        get => quickItemContainer_1;
        set
        {
            if (quickItemContainer_1 != value)
            {
                quickItemContainer_1 = value;
                OnQuickSlotChanged?.Invoke();
            }
        }
    }

    private ItemContainer quickItemContainer_2;
    public ItemContainer QuickItemContainer_2
    {
        get => quickItemContainer_2;
        set
        {
            if (quickItemContainer_2 != value)
            {
                quickItemContainer_2 = value;
                OnQuickSlotChanged?.Invoke();
            }
        }
    }

    private ItemContainer quickItemContainer_3;
    public ItemContainer QuickItemContainer_3
    {
        get => quickItemContainer_3;
        set
        {
            if (quickItemContainer_3 != value)
            {
                quickItemContainer_3 = value;
                OnQuickSlotChanged?.Invoke();
            }
        }
    }

    [SerializeField] private ObjectPool boxProxyColliderPool;
    [SerializeField] private ObjectPool circleProxyColliderPool;
    [SerializeField] private ObjectPool skillPool;
    [SerializeField] private ObjectPool linearDynamicSkillPool;
    [SerializeField] private ObjectPool takeHitVFX_Pool;

    /// <summary>
    /// 원형 대리 충돌체를 반환하는 함수
    /// </summary>
    public BoxProxyCollider GetBoxProxyCollider()
    {
        return boxProxyColliderPool.Burrow<BoxProxyCollider>();
    }

    /// <summary>
    /// 원형 대리 충돌체를 반환하는 함수
    /// </summary>
    public CircleProxyCollider GetCircleProxyCollider()
    {
        return circleProxyColliderPool.Burrow<CircleProxyCollider>();
    }

    /// <summary>
    /// 스킬 인스턴스를 반환하는 함수
    /// </summary>
    public Skill GetSkill()
    {
        return skillPool.Burrow<Skill>();
    }

    /// <summary>
    /// 선형 동적 스킬 인스턴스를 반환하는 함수
    /// </summary>
    public LinearDynamicSkill GetLinearDynamicSkill()
    {
        return linearDynamicSkillPool.Burrow<LinearDynamicSkill>();
    }

    /// <summary>
    /// 피격 이펙트 인스턴스를 반환하는 함수
    /// </summary>
    public TakeHitVFX GetTakeHitVFX()
    {
        return takeHitVFX_Pool.Burrow<TakeHitVFX>();
    }
}
