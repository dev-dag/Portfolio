using UnityEngine;

public class HealthSlot : MonoBehaviour
{
    [SerializeField] private float sizeAnimSpeed = 0.05f;
    [SerializeField] private float posAnimSpeed = 0.05f;

    public void Destroy()
    {
        DestroyProgress();
    }

    private async Awaitable DestroyProgress()
    {
        while (this.transform.localScale.x > 0)
        {
            this.transform.position += Vector3.up * 0.05f;
            this.transform.localScale -= Vector3.one * 0.05f;

            await Awaitable.NextFrameAsync();
        }

        Destroy(this.gameObject);
    }
}
