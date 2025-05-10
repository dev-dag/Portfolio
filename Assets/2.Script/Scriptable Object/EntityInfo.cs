using UnityEngine;

[CreateAssetMenu(fileName = "Entity Info", menuName = "Scriptable Object/Entity Info", order = 0)]
public class EntityInfo : ScriptableObject
{
    public int Hp { get => hp; }
    public float JumpPower { get => jumpPower; }
    public float Speed { get => speed; }

    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private int hp;
}
