using UnityEngine;

public interface ICombatable
{
    /// <summary>
    /// ü���� �����ϴ� �Լ�
    /// </summary>
    /// <param name="damage">���� ��ġ</param>
    public void TakeHit(int damage, BaseObject hitter);
}
