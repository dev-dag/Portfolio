using UnityEngine;

public class View : MonoBehaviour
{
    public bool IsInit { get; private set; } = false;

    /// <summary>
    /// View를 초기화하는 함수
    /// </summary>
    public virtual void Init()
    {
        Hide();
        IsInit = true;
    }

    /// <summary>
    /// View를 보여주는 함수
    /// </summary>
    public virtual void Show()
    {
        if (this.gameObject.activeInHierarchy == false)
        {
            this.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// View를 숨기는 함수
    /// </summary>
    public virtual void Hide()
    {
        if (this.gameObject.activeInHierarchy == true)
        {
            this.gameObject.SetActive(false);
        }
    }
}
