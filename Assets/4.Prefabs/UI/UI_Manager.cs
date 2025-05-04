using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : BaseObject
{
    [SerializeField] private Image fadeImage;

    [Space(30f)]
    public ObjectPool overheadUI_Pool;
    public Dialog dialog;
    public Inventory inventory;
    public QuickSlot quickSlot;
    public ItemInfo itemInfo;
    public PlayerInfoPreview playerInfoPreview;
    public SkillView skillView;

    private Awaitable fadeAwaiter = null;

    protected override void Start()
    {
        base.Start();

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        dialog.gameObject.SetActive(false);
        inventory.gameObject.SetActive(false);
        quickSlot.gameObject.SetActive(true);
        itemInfo.gameObject.SetActive(false);
        playerInfoPreview.gameObject.SetActive(true);
        skillView.gameObject.SetActive(true);
    }

    public void Init()
    {
        inventory.Init();

        inventory.AddItem(0, 1);
        inventory.AddItem(1, 1);
        inventory.AddItem(2, 1);
        inventory.AddItem(3, 3);
    }

    public void FadeIn(float duration = 1f)
    {
        if (fadeAwaiter != null)
        {
            fadeAwaiter.Cancel();
        }

        fadeAwaiter = Fade(1f, 0f, duration);
    }

    public void FadeOut(float duration = 1f)
    {
        if (fadeAwaiter != null)
        {
            fadeAwaiter.Cancel();
        }

        fadeAwaiter = Fade(0f, 1f, duration);
    }

    public void ShowUI_ForCinematic(bool isShown = false)
    {
        if (isShown == false)
        {
            inventory.Disable();
        }

        quickSlot.gameObject.SetActive(isShown);
        skillView.gameObject.SetActive(isShown);
        playerInfoPreview.gameObject.SetActive(isShown);
    }

    private async Awaitable Fade(float fromAlpha, float toAlpha, float duration)
    {
        float time = 0f;

        fadeImage.color = new Color(0f, 0f, 0f, fromAlpha);

        while (time < duration)
        {
            time += Time.deltaTime;
            fadeImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(fromAlpha, toAlpha, time / duration));
            await Awaitable.NextFrameAsync();
        }

        fadeImage.color = new Color(0f, 0f, 0f, toAlpha);

        fadeAwaiter = null;
    }
}
