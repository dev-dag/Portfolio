using UnityEngine;

namespace Monster
{
    public class Monster : BaseObject
    {
        [SerializeField] protected MonsterInfo info;
        protected float hp;

        public void Init()
        {
            hp = info.hp;
        }
    }

}