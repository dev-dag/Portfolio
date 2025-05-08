using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Scriptable Object/Skill Data")]
public class SkillData : ScriptableObject
{
    public enum SkillType
    {
        Normal = 0,
        LinearDynamic = 1, // 수직 이동하는 스킬
    }

    public enum ParryType
    {
        Normal = 0,
        Parryable = 1, // 패리 가능한 스킬
        Parried = 2, // 패리 대상 스킬
    }

    public enum SkillCollisionType
    {
        None = 0,
        Box,
        Circle,
    }

    public Sprite icon;

    [Space(30f)]
    public SkillType skillType;
    public SkillCollisionType collisionType;
    public ParryType parryType;
    public int additionalDamage;
    public float coolTime = 0.1f;
    public Vector2 VFX_Offset;
    public RuntimeAnimatorController animationController;
    public Vector2 colliderOffset;
    public int castingLayer;

    [Space(30f)]
    public List<AudioClip> SFX_Clips;

    [Space(30f)]
    public bool useLifeTime = true;
    public float lifeTime = 3f;

    [Space(20f)]
    [Tooltip("when collision type is box only.")] public Vector2 colliderSize;

    [Space(20f)]
    [Tooltip("when collision type is circle only.")] public float radius;
}
