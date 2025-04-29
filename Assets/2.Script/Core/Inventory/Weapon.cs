using Database_Table;
using FluentBehaviourTree;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Weapon : ItemContainer
{
    public WeaponInfo WeaponInfo { get; private set; }

    protected Weapon weapon;

    /// <returns>무기 타입이 아닌 경우 초기화 실패. false 반환</returns>
    public override bool Init(Item item, int amount)
    {
        if (item.Type == 1)
        {
            base.Init(item, amount);
            WeaponInfo = GameManager.Instance.LoadItemInfo<WeaponInfo>(item.ID);

            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual Skill[] GetSkills()
    {
        return null;
    }

    public virtual IBehaviourTreeNode GetSkill_0_BehaviourTree(Player player)
    {
        return null;
    }

    public virtual IBehaviourTreeNode GetSkill_1_BehaviourTree(Player player)
    {
        return null;
    }

    public virtual IBehaviourTreeNode GetSkill_2_BehaviourTree(Player player)
    {
        return null;
    }
}
