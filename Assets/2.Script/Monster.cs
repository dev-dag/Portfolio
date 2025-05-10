using UnityEngine;

namespace Monster
{
    public class Monster : Entity
    {
        [SerializeField] protected MonsterInfo info;
        protected int hp;

        public void Init()
        {
            hp = info.hp;
        }
    }

}