public class SkillView : BaseObject
{
    public SkillSlotView skill_0_slotView;
    public SkillSlotView skill_1_slotView;
    public SkillSlotView skill_2_slotView;

    /// <summary>
    /// 무기에 포함된 스킬로 모든 슬롯을 설정하는 함수. 비우고 싶으면 null을 넣으면 됨.
    /// </summary>
    /// <param name="weaponInfo"></param>
    public void SetSkill(Weapon weapon)
    {
        if (weapon == null)
        {
            skill_0_slotView.Set(null);
            skill_1_slotView.Set(null);
            skill_2_slotView.Set(null);
        }
        else
        {
            Skill[] skills = weapon.GetSkills();

            skill_0_slotView.Set(skills[0]);
            skill_1_slotView.Set(skills[1]);
            skill_2_slotView.Set(skills[2]);
        }
    }

    /// <summary>
    /// A스킬 슬롯을 스킬 데이터로 세팅하는 함수. 비우고 싶으면 null을 넣으면 됨.
    /// </summary>
    public void SetSkill_0(Skill skill)
    {
        skill_0_slotView.Set(skill);
    }

    /// <summary>
    /// A스킬 슬롯을 스킬 데이터로 세팅하는 함수. 비우고 싶으면 null을 넣으면 됨.
    /// </summary>
    public void SetSkill_1(Skill skill)
    {
        skill_1_slotView.Set(skill);
    }

    /// <summary>
    /// A스킬 슬롯을 스킬 데이터로 세팅하는 함수. 비우고 싶으면 null을 넣으면 됨.
    /// </summary>
    public void SetSkill_2(Skill skill)
    {
        skill_2_slotView.Set(skill);
    }
}
