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
    [Tooltip("when collision type is box only.")] public Vector2 size;
    [Tooltip("when collision type is circle only.")] public float radius;
    public Vector2 offset;
    public RuntimeAnimatorController animationController;
}
