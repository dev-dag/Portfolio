using UnityEngine;

public class Skill
{
    public float NextCoolDownTime { get; private set; }

    private SkillData data;
    private int weaponDamage;

    public Skill(SkillData data, int weaponDamage)
    {
        this.data = data;
        this.weaponDamage = weaponDamage;
    }

    public bool TryOperate(Vector2 position, Quaternion rotation, int layer, BaseObject caller)
    {
        if (Time.time < NextCoolDownTime) // 쿨타임이 아직 지나지 않은 경우
        {
            return false;
        }
        else
        {
            if (data.skillType == SkillData.SkillType.Normal)
            {
                var skillAction = GameManager.Instance.combatSystem.GetSkillAction();
                skillAction.Init(weaponDamage, position, rotation, layer, data, caller);
                skillAction.Enable();
            }
            else
            {
                var skillAction = GameManager.Instance.combatSystem.GetLinearDynamicSkillAction();
                skillAction.Init(weaponDamage, position, rotation, layer, data, caller);
                skillAction.Enable();
            }

            NextCoolDownTime = Time.time + data.coolTime;

            return true;
        }
    }

    public bool IsOperatable()
    {
        if (Time.time < NextCoolDownTime) // 쿨타임이 아직 지나지 않은 경우
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
