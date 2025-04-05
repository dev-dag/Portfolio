using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Scriptable Object/Skill Data")]
public class SkillData : ScriptableObject
{
    public enum SkillType
    {
        Normal = 0,
        Parryable = 1, // �и� ������ ��ų
        Parried = 2, // �и� ��� ��ų
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
