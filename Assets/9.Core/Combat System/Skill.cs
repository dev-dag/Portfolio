using UnityEngine;
using UnityEngine.UIElements;

public class Skill
{
    public float NextCoolDownTime { get; private set; }
    public SkillData Data { get => data; }

    private SkillData data;
    private int weaponDamage;

    public Skill(SkillData data, int weaponDamage)
    {
        this.data = data;
        this.weaponDamage = weaponDamage;
    }

    public bool TryOperate(Vector2 position, Quaternion rotation, int layer, Entity caller)
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
                skillAction.Init(weaponDamage, position, rotation, layer, data, caller, new SkillAction.Option());
                skillAction.Enable();

                NextCoolDownTime = Time.time + data.coolTime;

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool TryOperateWithFollow(Transform transform, Quaternion rotation, int layer, Entity caller)
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
                skillAction.Init(weaponDamage, Vector2.zero, rotation, layer, data, caller, new SkillAction.Option(transform));
                skillAction.Enable();

                NextCoolDownTime = Time.time + data.coolTime;

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool TryOperateLinearDynamic(Vector2 position, Quaternion rotation, int layer, Entity caller, Vector2 direction, float speed)
    {
        if (Time.time < NextCoolDownTime) // 쿨타임이 아직 지나지 않은 경우
        {
            return false;
        }
        else
        {
            if (data.skillType == SkillData.SkillType.LinearDynamic)
            {
                var skillAction = GameManager.Instance.combatSystem.GetLinearDynamicSkillAction();
                skillAction.Init(weaponDamage, position, rotation, layer, data, caller, direction, speed);
                skillAction.Enable();

                NextCoolDownTime = Time.time + data.coolTime;

                return true;
            }
            else
            {
                return false;
            }
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
