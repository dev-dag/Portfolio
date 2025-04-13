using UnityEngine;

public class Skill : PoolingObject, ICombatAnimatorEventListener
{
    [SerializeField] private Animator effectAnimator;

    private ProxyCollider proxyCollider;

    [SerializeField] private SkillData data;
    private int layer = 0;

    public void Init(Vector2 worldPosition, int layer, SkillData data)
    {
        this.data = data;
        this.transform.position = worldPosition;
        this.layer = layer;

        effectAnimator.transform.localPosition = data.VFX_Offset;
        effectAnimator.runtimeAnimatorController = data.animationController;

        this.gameObject.SetActive(false);
    }

    public override void Enable()
    {
        base.Enable();

        Invoke("Return", 3f);
    }

    /// <summary>
    /// 애니메이터에서 이벤트로 호출되는 피격판정을 시작하는 함수
    /// </summary>
    void ICombatAnimatorEventListener.StartHit()
    {
        switch (data.collisionType)
        {
            case SkillData.SkillCollisionType.Box:
            {
                BoxProxyCollider boxProxyCollider = GameManager.Instance.combatSystem.GetBoxProxyCollider();
                proxyCollider = boxProxyCollider;

                boxProxyCollider.Init(this.transform.position, data.colliderOffset, data.colliderSize, layer, OnHit);

                proxyCollider.Enable();

                break;
            }
            case SkillData.SkillCollisionType.Circle:
            {
                CircleProxyCollider circleProxyCollider = GameManager.Instance.combatSystem.GetCircleProxyCollider();
                proxyCollider = circleProxyCollider;

                circleProxyCollider.Init(this.transform.position, data.colliderOffset, data.radius, layer, OnHit);

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
    void ICombatAnimatorEventListener.StopHit()
    {
        proxyCollider.Return();
        proxyCollider = null;
    }

    /// <summary>
    /// 피격 범위에 들어온 대상의 체력을 감소시키는 함수.
    /// </summary>
    /// <param name="combatInterface">체력을 감소시키는 함수를 제공하는 인터페이스</param>
    protected void OnHit(ICombatable combatInterface, Rigidbody2D hitRB)
    {
        combatInterface.TakeHit(data.damage, hitRB);
    }
}
