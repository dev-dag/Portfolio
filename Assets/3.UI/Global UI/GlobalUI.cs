using UnityEngine;
using UnityEngine.UI;

public class GlobalUI : View
{
    public Fade Fade { get => fade; }
    public OptionView OptionView { get => optionView; }

    [SerializeField] private Fade fade;
    [SerializeField] private OptionView optionView;

    public override void Init()
    {
        fade.Init();
        optionView.Init();

        fade.Show();
        optionView.Hide();

        base.Init();
    }
}
