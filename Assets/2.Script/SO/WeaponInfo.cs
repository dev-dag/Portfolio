using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Info", menuName = "Scriptable Object/Item Info/Weapon Info")]
public class WeaponInfo : ItemInfoData
{
    public int damage;

    [Space(30f), Header("Skill_0")]
    public string AnimationStateName_0;
    public SkillData SkillData_0;

    [Space(30f), Header("Skill_1")]
    public string AnimationStateName_1;
    public SkillData SkillData_1;

    [Space(30f), Header("Skill_2")]
    public string AnimationStateName_2;
    public SkillData SkillData_2;
}