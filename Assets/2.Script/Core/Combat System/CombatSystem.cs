using System;
using UnityEngine;

public class CombatSystem : BaseObject
{
    public enum EntityType
    {
        Player,
        Enemy
    }

    [SerializeField] private ObjectPool boxProxyColliderPool;
    [SerializeField] private ObjectPool circleProxyColliderPool;
    [SerializeField] private ObjectPool skillPool;
    [SerializeField] private ObjectPool linearDynamicSkillPool;
    [SerializeField] private ObjectPool takeHitVFX_Pool;

    /// <summary>
    /// ���� �븮 �浹ü�� ��ȯ�ϴ� �Լ�
    /// </summary>
    public BoxProxyCollider GetBoxProxyCollider()
    {
        return boxProxyColliderPool.Burrow<BoxProxyCollider>();
    }

    /// <summary>
    /// ���� �븮 �浹ü�� ��ȯ�ϴ� �Լ�
    /// </summary>
    public CircleProxyCollider GetCircleProxyCollider()
    {
        return circleProxyColliderPool.Burrow<CircleProxyCollider>();
    }

    /// <summary>
    /// ��ų �ν��Ͻ��� ��ȯ�ϴ� �Լ�
    /// </summary>
    public Skill GetSkill()
    {
        return skillPool.Burrow<Skill>();
    }

    /// <summary>
    /// ���� ���� ��ų �ν��Ͻ��� ��ȯ�ϴ� �Լ�
    /// </summary>
    public LinearDynamicSkill GetLinearDynamicSkill()
    {
        return linearDynamicSkillPool.Burrow<LinearDynamicSkill>();
    }

    /// <summary>
    /// �ǰ� ����Ʈ �ν��Ͻ��� ��ȯ�ϴ� �Լ�
    /// </summary>
    public TakeHitVFX GetTakeHitVFX()
    {
        return takeHitVFX_Pool.Burrow<TakeHitVFX>();
    }
}
