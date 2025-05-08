using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPreview : BaseObject
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private GameObject originHealthSlotPrefab;
    [SerializeField] private Transform healthSlotLayoutGroupTr;

    private List<GameObject> healthInstances = new List<GameObject>();

    public void Init(int hp, Sprite weaponSprite)
    {
        for (int count = 1; count <= hp; count++)
        {
            GameObject newHealth = Instantiate(originHealthSlotPrefab, healthSlotLayoutGroupTr);
            newHealth.SetActive(true);

            healthInstances.Add(newHealth);
        }

        SetWeaponSprite(weaponSprite);
    }

    public void SetWeaponSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            weaponImage.sprite = null;
            weaponImage.color = new Color(weaponImage.color.r, weaponImage.color.g, weaponImage.color.b, 0f);
        }
        else
        {
            weaponImage.sprite = sprite;
            weaponImage.color = new Color(weaponImage.color.r, weaponImage.color.g, weaponImage.color.b, 1f);
        }
    }

    public void Increase(int amount)
    {
        for (int count = 1; count <= amount; count++)
        {
            GameObject newHealth = Instantiate(originHealthSlotPrefab, healthSlotLayoutGroupTr);
            newHealth.SetActive(true);

            healthInstances.Add(newHealth);
        }
    }

    public void Decrease(int amount)
    {
        for (int count = 1; count <= amount; count++)
        {
            GameObject recentHealth = healthInstances[healthInstances.Count - 1];

            healthInstances.Remove(recentHealth);

            recentHealth.GetComponent<HealthSlot>().Destroy();
        }
    }
}