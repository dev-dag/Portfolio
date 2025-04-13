using UnityEngine;

namespace Monster
{
    public class Monster : BaseObject, ICombatable
    {
        [SerializeField] protected MonsterInfo info;
        protected float hp;

        public void Init()
        {
            hp = info.hp;
        }

        void ICombatable.TakeHit(float damage)
        {
            hp -= damage;
        }
    }

}