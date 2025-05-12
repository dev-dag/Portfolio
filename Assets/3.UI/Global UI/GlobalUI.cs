using UnityEngine;
using UnityEngine.UI;

public class GlobalUI : MonoBehaviour
public class GlobalUI : View
{
    public Fade Fade { get => fade; }

    [SerializeField] private Fade fade;
    public override void Init()
    {
        fade.Init();
        
        base.Init();
    }
}
