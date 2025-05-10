using UnityEngine;

/// <summary>
/// Weapon클래스를 상속받아 BT를 제공하는 무기 아이템 정의. 정수 값은 무기의 ID와 매칭됨.
/// </summary>
public enum WeaponEnum : int
{
    None = -1,
    Sword = 0,
    Dagger = 1,
    Axe = 2,
}