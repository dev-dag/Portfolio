public class SkillView : BaseObject
{
    public SkillSlotView skill_0_slotView;
    public SkillSlotView skill_1_slotView;
    public SkillSlotView skill_2_slotView;

    /// <summary>
    /// ���⿡ ���Ե� ��ų�� ��� ������ �����ϴ� �Լ�. ���� ������ null�� ������ ��.
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
    /// A��ų ������ ��ų �����ͷ� �����ϴ� �Լ�. ���� ������ null�� ������ ��.
    /// </summary>
    public void SetSkill_0(Skill skill)
    {
        skill_0_slotView.Set(skill);
    }

    /// <summary>
    /// A��ų ������ ��ų �����ͷ� �����ϴ� �Լ�. ���� ������ null�� ������ ��.
    /// </summary>
    public void SetSkill_1(Skill skill)
    {
        skill_1_slotView.Set(skill);
    }

    /// <summary>
    /// A��ų ������ ��ų �����ͷ� �����ϴ� �Լ�. ���� ������ null�� ������ ��.
    /// </summary>
    public void SetSkill_2(Skill skill)
    {
        skill_2_slotView.Set(skill);
    }
}
