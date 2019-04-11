/*
 *  Heart health bar UI element filling. Adapted from the free Unity store asset 'Simple Health Heart System' by ariel oliveira [o.arielg@gmail.com]
 */

using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
   
    private static GameObject[] heartContainers;
    private static Image[] heartFills;

    public Transform heartsParent;
    public GameObject heartContainerPrefab;

    private void Start()
    {
        heartContainers = new GameObject[Stats.MAX_TOTAL_HEALTH];
        heartFills = new Image[Stats.MAX_TOTAL_HEALTH];

        Stats.onHealthChangedCallback += UpdateHeartsHUD;
        InstantiateHeartContainers();
        UpdateHeartsHUD();
    }

    public void UpdateHeartsHUD()
    {
        SetHeartContainers();
        SetFilledHearts();
    }

    void SetHeartContainers()
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < Stats.MaxHp)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }

    void SetFilledHearts()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < Stats.Hp)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }

        if (Stats.Hp % 1 != 0)
        {
            int lastPos = Mathf.FloorToInt(Stats.Hp);
            heartFills[lastPos].fillAmount = Stats.Hp % 1;
        }
    }

    void InstantiateHeartContainers()
    {
        for (int i = 0; i < Stats.MAX_TOTAL_HEALTH; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
        }
    }
}
