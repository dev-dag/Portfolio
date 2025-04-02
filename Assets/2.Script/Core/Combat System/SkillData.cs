using UnityEngine;

[CreateAssetMenu(fileName = "Skill Data", menuName = "Scriptable Object/Skill Data")]
public class SkillData : ScriptableObject
{
    public enum SkillCollisionType
    {
        Box = 0,
        Circle,
    }

    public SkillCollisionType collisionType;
    public float damage;
    [Tooltip("when collision type is box only.")] public Vector2 size;
    [Tooltip("when collision type is circle only.")] public float radius;
    public Vector2 offset;
    public RuntimeAnimatorController animationController;
}
