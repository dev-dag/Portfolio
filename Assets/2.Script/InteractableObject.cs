using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractableObject : BaseObject
{
    protected override void Awake()
    {
        base.Awake();

        if (GetComponent<Collider2D>().isTrigger == false)
        {
            EDebug.LogWarning("인자로 설정된 충돌체가 트리거 타입이 아님.");
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
