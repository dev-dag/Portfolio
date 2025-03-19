using UnityEngine;

public abstract class SingleTon<T> : MonoBehaviour where T : SingleTon<T>
{
    public static T Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            else
            {
                throw new System.NullReferenceException();
            }
        }
    }

    private static T instance;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = (T)this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
