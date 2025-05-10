using UnityEngine;
using UnityEngine.UI;

public class SkillSlotView : BaseObject
{
    [SerializeField] private Image image;
    [SerializeField] private Image coolTimeImage;

    private Skill currentSkill;

    protected override void Awake()
    {
        base.Awake();

        image.gameObject.SetActive(false);
    }

    /// <summary>
    /// 뷰에 스킬을 할당하는 함수
    /// </summary>
    public void Set(Skill skill)
    {
        currentSkill = skill;

        if (currentSkill != null)
        {
            image.sprite = currentSkill.Data.icon;
            image.gameObject.SetActive(true);
            coolTimeImage.fillAmount = 0f;
        }
        else
        {
            image.sprite = null;
            image.gameObject.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (currentSkill == null)
        {
            return;
        }

        if (currentSkill.NextCoolDownTime > Time.time)
        {
            float operatedTime = currentSkill.NextCoolDownTime - currentSkill.Data.coolTime;

            coolTimeImage.fillAmount = 1f - (Time.time - operatedTime) / currentSkill.Data.coolTime;
        }
        else
        {
            coolTimeImage.fillAmount = 0f;
        }
    }
}
