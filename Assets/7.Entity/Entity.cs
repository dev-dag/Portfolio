using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public bool IsInit { get; protected set; } = false;

    public EntityInfo Info { get => info; }
    public int HP { get => hp; }
    public bool IsDead { get => isDead; }

    [SerializeField] protected EntityInfo info;
    protected int hp;
    protected bool isDead;

    public virtual void Init()
    {
        hp = Info.Hp;
        isDead = false;
        IsInit = true;

        return;
    }
}