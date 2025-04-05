using UnityEngine;

public class Monster : BaseObject, ICombatable
{
    [SerializeField] private MonsterInfo info;
    public float hp;

    public void Init()
    {
        hp = info.hp;
    }

    void ICombatable.TakeHit(float damage)
    {
        hp -= damage;
    }
}
