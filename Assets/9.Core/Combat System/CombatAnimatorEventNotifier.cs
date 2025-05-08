using UnityEngine;

public class CombatAnimatorEventNotifier : MonoBehaviour
{
    public GameObject combatAnimatorEventListener;

    private ICombatAnimatorEventListener _combatAnimatorEventListener;

    private void Awake()
    {
        _combatAnimatorEventListener = combatAnimatorEventListener.GetComponent<ICombatAnimatorEventListener>();
    }

    public void StartHit()
    {
        _combatAnimatorEventListener?.StartHit();
    }

    public void StopHit()
    {
        _combatAnimatorEventListener?.StopHit();
    }
}
