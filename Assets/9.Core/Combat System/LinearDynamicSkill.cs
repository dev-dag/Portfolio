using UnityEngine;

public class LinearDynamicSkill : SkillAction
{
    private Vector2 dir;
    private float speed;

    public void Init(int weaponDamage, Vector2 position, Quaternion rotation, int layer, SkillData data, BaseObject caller, Vector2 dir, float speed)
    {
        Init(weaponDamage, position, rotation, layer, data, caller, new Option());

        this.dir = dir;
        this.speed = speed;
    }

    public override void Enable()
    {
        base.Enable();
    }

    protected override void Update()
    {
        base.Update();

        if (dir != Vector2.zero)
        {
            this.transform.position += new Vector3(dir.x * speed * Time.deltaTime, dir.y * speed * Time.deltaTime, 0f);

            if (proxyCollider != null) // ���� ó���� ���� Return�Լ��� ȣ��� ��� proxyCollider �ν��Ͻ��� null�� �� ����.
            {
                proxyCollider.transform.position = this.transform.position;
            }
        }
    }

    protected override void OnHit(Collider2D collision)
    {
        base.OnHit(collision);

        if (collision.attachedRigidbody.gameObject.layer == LayerMask.NameToLayer(GameManager.PLATFORM_LAYER_NAME)
            && IsReturned == false)
        {
            Return();
        }
    }
}
