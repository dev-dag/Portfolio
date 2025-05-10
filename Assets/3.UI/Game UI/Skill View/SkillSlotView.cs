using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotView : View
{
    [SerializeField] private Image image;
    [SerializeField] private Image coolTimeImage;

    private Skill currentSkill;

    public override void Init()
    {
        base.Init();

        image.gameObject.SetActive(false);
        image.sprite = null;
        coolTimeImage.fillAmount = 0f;
        currentSkill = null;
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

    private void Update()
    {
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
