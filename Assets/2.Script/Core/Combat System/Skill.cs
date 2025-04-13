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
    /// �ִϸ����Ϳ��� �̺�Ʈ�� ȣ��Ǵ� �ǰ������� �����ϴ� �Լ�
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
                Debug.LogError("��ų ������ ���� �߻�");

                break;
            }
        }
    }

    /// <summary>
    /// �ִϸ����Ϳ��� �̺�Ʈ�� ȣ��Ǵ� �ǰ������� �����ϴ� �Լ�
    /// </summary>
    void ICombatAnimatorEventListener.StopHit()
    {
        proxyCollider.Return();
        proxyCollider = null;
    }

    /// <summary>
    /// �ǰ� ������ ���� ����� ü���� ���ҽ�Ű�� �Լ�.
    /// </summary>
    /// <param name="combatInterface">ü���� ���ҽ�Ű�� �Լ��� �����ϴ� �������̽�</param>
    protected void OnHit(ICombatable combatInterface, Rigidbody2D hitRB)
    {
        combatInterface.TakeHit(data.damage, hitRB);
    }
}
