using UnityEngine;

public class LinearDynamicSkill : Skill
{
    private Vector2 velocity;

    public void Init(int weaponDamage, Vector2 position, Quaternion rotation, int layer, SkillData data, BaseObject caller, Vector2 velocity)
    {
        Init(weaponDamage, position, rotation, layer, data, caller);

        this.velocity = velocity;
    }

    public override void Enable()
    {
        base.Enable();
    }

    protected override void Update()
    {
        base.Update();

        if (velocity != Vector2.zero)
        {
            this.transform.position += new Vector3(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime, 0f);

            if (proxyCollider != null) // 물리 처리로 인해 Return함수가 호출된 경우 proxyCollider 인스턴스가 null일 수 있음.
            {
                proxyCollider.transform.position = this.transform.position;
            }
        }
    }

    protected override void OnHit(Collider2D collision)
    {
        base.OnHit(collision);

        if (collision.attachedRigidbody.gameObject.layer == LayerMask.NameToLayer(GameManager.PLATFORM_LAYER_NAME))
        {
            Return();
        }
    }
}
