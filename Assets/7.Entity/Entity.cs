using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    protected EntityInfo Info { get => info; }
    protected int HP { get => hp; }
    protected bool IsDead { get => isDead; }

    [SerializeField] protected EntityInfo info;
    protected int hp;
    protected bool isDead;

    public virtual void Init()
    {
        hp = Info.Hp;
        isDead = false;

        return;
    }
}