using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Info", menuName = "Scriptable Object/Player Info", order = 0)]
public class PlayerInfo : ScriptableObject
{
    public float speed;
    public float jumpPower;
    public float maxHP;
    public float HP;
    public bool isDead;
    public bool freazeMove;

    public void Init()
    {
        isDead = false;
        freazeMove = false;
        HP = maxHP;
    }
}
