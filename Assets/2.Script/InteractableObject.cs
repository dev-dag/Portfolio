using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractableObject : BaseObject
{
    protected override void Awake()
    {
        base.Awake();

        if (GetComponent<Collider2D>().isTrigger == false)
        {
            EDebug.LogWarning("���ڷ� ������ �浹ü�� Ʈ���� Ÿ���� �ƴ�.");
            return;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        
    }
}
