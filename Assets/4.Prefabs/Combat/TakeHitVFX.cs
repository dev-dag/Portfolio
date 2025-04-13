using UnityEngine;

public class TakeHitVFX : PoolingObject
{
    [SerializeField] private Animator anim;

    private static readonly int ANIM_CLIP_HASH = Animator.StringToHash("ACP_Take Hit");

    public void Init(Vector3 origin)
    {
        this.transform.position = origin;
    }

    public override void Enable()
    {
        base.Enable();

        anim.gameObject.SetActive(true);
        anim.Play(ANIM_CLIP_HASH);

        Invoke("Return", 3f);
    }

    public override void Return()
    {
        base.Return();

        anim.gameObject.SetActive(false);
    }
}
