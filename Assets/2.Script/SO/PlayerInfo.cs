using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Info", menuName = "Scriptable Object/Player Info", order = 0)]
public class PlayerInfo : ScriptableObject
{
    public float speed;
    public float jumpPower;
    public int maxHP;
    public int HP;
    public bool isDead;
    public bool freazeMove;
    public Dictionary<int, int> ownItems;

    public void Init()
    {
        isDead = false;
        freazeMove = false;
        HP = maxHP;
    }
}
