using UnityEngine;
using UnityEngine.UI;

public class ShopSlotButton : MonoBehaviour
{
    public Item item;
    public Sprite image;
    public int amountToSell = 1;
    public int pricePerUnit = 5;

    public Image iconImage;
    public Text priceText;
    public int tower = -1;

    public Image[] crystalIcons; // Индексы 0,1,... отображают руны
    public Color redColor = Color.red;
    public Color blueColor = Color.cyan;
    public Color missingColor = Color.black;

    public int redOreID = 100;
    public int blueOreID = 101;

    private MovementInput player;
    private PlayerInteraction playerint;
    private Inventory inventory;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<MovementInput>();
        playerint = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInteraction>();
        inventory = player?.GetComponent<Inventory>();

        if (item != null && iconImage != null)
            iconImage.sprite = item.icon;
        if (tower >= 0)
            iconImage.sprite = image;

        if (priceText != null)
            priceText.text = (pricePerUnit * amountToSell).ToString();

        GetComponent<Button>()?.onClick.AddListener(BuyItem);
    }

    void Update()
    {
        UpdatePriceDisplay(player.GetComponent<MovementInput>().moneyBalance);
        if (tower >= 0 && crystalIcons != null && crystalIcons.Length > 0)
        {
            UpdateCrystalDisplay();
        }
    }

    void BuyItem()
    {
        if (tower >= 0)
        {
            playerint.BuildTower(tower, pricePerUnit);
        }
        else
        {
            if (player == null || item == null) return;

            ItemInstance instance = new ItemInstance();
            instance.itemData = item;
            instance.amount = amountToSell;

            player.Buy(instance, amountToSell, pricePerUnit);
        }
    }

    public void UpdatePriceDisplay(int playerMoney)
    {
        priceText.text = pricePerUnit.ToString();
        priceText.color = playerMoney < pricePerUnit ? Color.red : Color.white;
    }

    void UpdateCrystalDisplay()
    {
        if (inventory == null || crystalIcons == null) return;

        int redRequired = 0;
        int blueRequired = 0;

        switch (tower)
        {
            case 0:
                blueRequired = 1;
                break;
            case 1:
                redRequired = 1;
                break;
            case 2:
                blueRequired = 2;
                break;
            case 3:
                redRequired = 2;
                break;
        }

        int redHave = inventory.GetItemCount(redOreID);
        int blueHave = inventory.GetItemCount(blueOreID);

        int index = 0;

        for (int i = 0; i < redRequired && index < crystalIcons.Length; i++, index++)
        {
            crystalIcons[index].color = i < redHave ? redColor : missingColor;
        }

        for (int i = 0; i < blueRequired && index < crystalIcons.Length; i++, index++)
        {
            crystalIcons[index].color = i < blueHave ? blueColor : missingColor;
        }

        // Оставшиеся кристаллы скрываем или делаем прозрачными
        for (; index < crystalIcons.Length; index++)
        {
            crystalIcons[index].color = Color.clear;
        }
    }
}