using UnityEngine;

public interface ICombatable
{
    /// <summary>
    /// 체력을 증감하는 함수
    /// </summary>
    /// <param name="damage">증감 수치</param>
    public void TakeHit(int damage, Entity hitter);
}
