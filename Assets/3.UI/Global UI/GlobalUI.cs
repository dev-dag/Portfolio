using UnityEngine;
using UnityEngine.UI;

public class GlobalUI : MonoBehaviour
{
    public Fade Fade { get => fade; }

    [SerializeField] private Fade fade;
}
