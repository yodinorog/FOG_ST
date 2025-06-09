using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("UI")]
    public GameObject pricePanel;
    public Text priceText;
    public GameObject equippedCheck;

    [Header("Данные предмета")]
    public int itemId;
    public int price;

    [Header("Тип слота")]
    public bool isColor = false;       // true — слот цвета, false — слот предмета
    public int colorIndex = 0;         // индекс цвета, если это цвет

    [Header("Связи")]
    public Button actionButton;
    public CharacterCustomization customization;

    private bool isOwned => itemId == 0 || PlayerPrefs.GetInt($"ItemOwned_{itemId}", 0) == 1;
    private bool isEquipped => PlayerPrefs.GetInt("EquippedItem", -1) == itemId;

    private void Start()
    {
        if (customization == null)
            customization = FindObjectOfType<CharacterCustomization>();

        actionButton.onClick.AddListener(OnButtonClick);
        UpdateUI();
    }


    private void UpdateUI()
    {
        bool owned = isColor ? IsColorOwned() : IsItemOwned();
        bool equipped = isColor ? IsColorEquipped() : IsItemEquipped();

        equippedCheck.SetActive(owned && equipped);

        if (!owned)
        {
            pricePanel.SetActive(true);
            priceText.text = price.ToString();
        }
        else
        {
            pricePanel.SetActive(false);
        }
    }

    private void OnButtonClick()
    {
        bool owned = isColor ? IsColorOwned() : IsItemOwned();

        if (!owned)
        {
            int crystalls = PlayerPrefs.GetInt("Crystalls", 0);
            if (crystalls >= price)
            {
                crystalls -= price;
                PlayerPrefs.SetInt("Crystalls", crystalls);
                if (isColor)
                    PlayerPrefs.SetInt($"ItemOwned_Color_{colorIndex}", 1);
                else
                    PlayerPrefs.SetInt($"ItemOwned_{itemId}", 1);

                PlayerPrefs.Save();
            }
            else
            {
                Debug.Log("Не хватает кристаллов");
                return;
            }
        }

        // Применяем цвет или предмет
        if (isColor)
        {
            customization.SetCharacterColor(colorIndex);
            PlayerPrefs.SetInt("CharacterColorIndex", colorIndex);
        }
        else
        {
            customization.EquipItem(itemId);
            PlayerPrefs.SetInt("EquippedItem", itemId);
        }

        PlayerPrefs.Save();

        foreach (ShopSlot slot in FindObjectsOfType<ShopSlot>())
            slot.UpdateUI();
    }

    private bool IsItemOwned() => PlayerPrefs.GetInt($"ItemOwned_{itemId}", 0) == 1;
    private bool IsItemEquipped() => PlayerPrefs.GetInt("EquippedItem", -1) == itemId;

    private bool IsColorOwned() => PlayerPrefs.GetInt($"ItemOwned_Color_{colorIndex}", 0) == 1;
    private bool IsColorEquipped() => PlayerPrefs.GetInt("CharacterColorIndex", -1) == colorIndex;
}