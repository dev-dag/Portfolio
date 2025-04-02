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
    /// �ִϸ����Ϳ��� �̺�Ʈ�� ȣ��Ǵ� �ǰ������� �����ϴ� �Լ�
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
                Debug.LogError("��ų ������ ���� �߻�");

                break;
            }
        }
    }

    /// <summary>
    /// �ִϸ����Ϳ��� �̺�Ʈ�� ȣ��Ǵ� �ǰ������� �����ϴ� �Լ�
    /// </summary>
    public void StopHit()
    {
        proxyCollider.Return();
    }

    /// <summary>
    /// �ǰ� ������ ���� ����� ü���� ���ҽ�Ű�� �Լ�.
    /// </summary>
    /// <param name="combatInterface">ü���� ���ҽ�Ű�� �Լ��� �����ϴ� �������̽�</param>
    protected void OnHit(ICombatable combatInterface)
    {
        combatInterface.TakeHit(data.damage);
    }
}
