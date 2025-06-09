using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellConfirmationPanel : MonoBehaviour
{
    public Text confirmationText; // или TMP_Text
    public Button yesButton;
    public Button noButton;

    public Image weaponIconImage; // ← новая ссылка на UI-иконку

    private Weapon weaponToSell;
    private Boots bootsToSell;
    private ItemInstance itemInstanceToSell;
    private MovementInput player;
    bool wep;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowW(Weapon weapon, ItemInstance itemInstance, MovementInput p)
    {
        wep = true;
        FindObjectOfType<AudioManager>().Play("Menu");

        weaponToSell = weapon;
        itemInstanceToSell = itemInstance;
        player = p;

        int profit = weapon.attackDamage * 20;
        confirmationText.text = $"->  {profit}?";

        if (weaponIconImage != null && weapon.icon != null)
        {
            weaponIconImage.sprite = weapon.icon;
            weaponIconImage.enabled = true;
        }

        gameObject.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(ConfirmSell);

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(CancelSell);
    }

    public void ShowB(Boots weapon, ItemInstance itemInstance, MovementInput p)
    {
        wep = false;
        FindObjectOfType<AudioManager>().Play("Menu");

        bootsToSell = weapon;
        itemInstanceToSell = itemInstance;
        player = p;

        int amount = player.inventory.GetAmount(itemInstance);
        int profit = (int)(weapon.speedBonus * 50f);
        confirmationText.text = amount > 0 ? $"->  {profit}?" : "Нет предмета для продажи!";

        if (weaponIconImage != null && weapon.icon != null)
        {
            weaponIconImage.sprite = weapon.icon;
            weaponIconImage.enabled = true;
        }

        yesButton.interactable = amount > 0;
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(ConfirmSell);

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(CancelSell);

        gameObject.SetActive(true);
    }

    void ConfirmSell()
    {
        int profit;
        if (wep)
            profit = weaponToSell.attackDamage * 20;
        else
            profit = (int)(bootsToSell.speedBonus * 50f);

        player.AddMoney(profit);
        player.ShowMessage($"Sold {(wep ? weaponToSell.weaponName : bootsToSell.itemName)} for {profit} coins.");

        player.inventory.RemoveItem(itemInstanceToSell);
        player.inventory.onInventoryChanged.Invoke();

        gameObject.SetActive(false);
        FindObjectOfType<AudioManager>().Play("Sell");
    }

    void CancelSell()
    {
        FindObjectOfType<AudioManager>().Play("Menu");
        gameObject.SetActive(false);
    }
}