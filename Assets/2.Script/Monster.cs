using UnityEngine;

namespace Monster
{
    public class Monster : BaseObject
    {
        [SerializeField] protected MonsterInfo info;
        protected int hp;

        public void Init()
        {
            hp = info.hp;
        }
    }

}