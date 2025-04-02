using UnityEngine;

public class Skill : PoolingObject
{
    [SerializeField] private Animator effectAnimator;

    private ProxyCollider proxyCollider;

    private SkillData data;
    private Vector2 worldPosition;

    public void Init(Vector2 worldPosition, SkillData data)
    {
        this.data = data;

        this.worldPosition = worldPosition;

        effectAnimator.runtimeAnimatorController = data.animationController;

        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 애니메이터에서 이벤트로 호출되는 피격판정을 시작하는 함수
    /// </summary>
    public void StartHit()
    {
        switch (data.collisionType)
        {
            case SkillData.SkillCollisionType.Box:
            {
                BoxProxyCollider boxProxyCollider = GameManager.Instance.combatSystem.GetBoxProxyCollider();
                proxyCollider = boxProxyCollider;

                boxProxyCollider.Init(worldPosition, data.size, OnHit);

                proxyCollider.gameObject.SetActive(false);

                proxyCollider.Enable();

                break;
            }
            case SkillData.SkillCollisionType.Circle:
            {
                CircleProxyCollider circleProxyCollider = GameManager.Instance.combatSystem.GetCircleProxyCollider();
                proxyCollider = circleProxyCollider;

                circleProxyCollider.Init(worldPosition, data.radius, OnHit);

                proxyCollider.gameObject.SetActive(false);

                proxyCollider.Enable();

                break;
            }
            default:
            {
                Debug.LogError("스킬 데이터 오류 발생");

                break;
            }
        }
    }

    /// <summary>
    /// 애니메이터에서 이벤트로 호출되는 피격판정을 종료하는 함수
    /// </summary>
    public void StopHit()
    {
        proxyCollider.Return();
    }

    /// <summary>
    /// 피격 범위에 들어온 대상의 체력을 감소시키는 함수.
    /// </summary>
    /// <param name="combatInterface">체력을 감소시키는 함수를 제공하는 인터페이스</param>
    protected void OnHit(ICombatable combatInterface)
    {
        combatInterface.TakeHit(data.damage);
    }
}
