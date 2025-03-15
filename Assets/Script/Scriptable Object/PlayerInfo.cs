using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Info", menuName = "Scriptable Object/Player Info", order = 0)]
public class PlayerInfo : ScriptableObject
{
    public float speed;
    public float jumpPower;
}
