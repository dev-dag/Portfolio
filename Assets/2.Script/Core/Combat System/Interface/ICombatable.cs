using UnityEngine;

public interface ICombatable
{
    /// <summary>
    /// ü���� �����ϴ� �Լ�
    /// </summary>
    /// <param name="damage">���� ��ġ</param>
    public void TakeHit(float damage, Rigidbody2D hitRB);
}
