using UnityEngine;
using UnityEngine.UI;

public class GlobalUI : BaseObject
{
    public Fade Fade { get => fade; }

    [SerializeField] private Fade fade;
}
