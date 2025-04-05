using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Scriptable Object/Skill Data")]
public class SkillData : ScriptableObject
{
    public enum SkillType
    {
        Normal = 0,
        Parryable = 1, // 패리 가능한 스킬
        Parried = 2, // 패리 대상 스킬
    }

    public enum SkillCollisionType
    {
        Box = 0,
        Circle,
    }

    public SkillCollisionType collisionType;
    public SkillType skillType;
    public float damage;
    public Vector2 VFX_Offset;
    public RuntimeAnimatorController animationController;
    public Vector2 colliderOffset;
    public int castingLayer;

    [Space(20f)]
    [Tooltip("when collision type is box only.")] public Vector2 colliderSize;

    [Space(20f)]
    [Tooltip("when collision type is circle only.")] public float radius;
}
