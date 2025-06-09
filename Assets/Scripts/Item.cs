using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public int id; // Уникальный идентификатор предмета
    public string itemName = "New Item"; // Название предмета
    public int max_stack; // Максимальное количество предметов в одной ячейке
    public Sprite icon = null; // Иконка предмета
    public bool isStackable = true; // Можно ли складывать в одну ячейку
    public GameObject prefab; // Префаб предмета

    public ItemType itemType; // Тип предмета (шлем, броня и т.д.)

    public virtual void Use(ItemInstance itemInstance)
    {
        Debug.Log("Using " + itemName);
    }
}

// Определение типов предметов
public enum ItemType
{
    Helmet,  // Шлем
    Armor,   // Броня
    Ore,    // Ноги
    Weapon,  // Оружие
    Potion,   // Зелье
    Book,
    Other
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class Weapon : Item
{
    public string weaponName;
    public GameObject model; // 3D модель оружия
    public int attackDamage; // Урон оружия

    public override void Use(ItemInstance itemInstance)
    {
        MovementInput player = FindObjectOfType<MovementInput>();

        GameObject shopUI = GameObject.Find("ShopUI");
        GameObject forgeUI = GameObject.Find("ForgeUI");
        GameObject mageUI = GameObject.Find("MageUI");

        // Если открыт магазин, кузница или маг — показать окно подтверждения продажи
        if ((shopUI != null && shopUI.activeSelf) || (mageUI != null && mageUI.activeSelf) || (forgeUI != null && forgeUI.activeSelf))
        {
            Transform panelTransform = player.sellConfirmationPanel.transform;
            if (panelTransform != null)
            {
                SellConfirmationPanel panel = panelTransform.GetComponent<SellConfirmationPanel>();
                if (panel != null)
                {
                    Debug.Log("Показ окна подтверждения продажи");
                    panel.ShowW(this, itemInstance, player);
                }
                else
                {
                    Debug.LogWarning("Компонент SellConfirmationUI не найден на объекте");
                }
            }
            else
            {
                Debug.LogWarning("Объект SellConfirmationUI не найден в иерархии игрока");
            }
            return;
        }

        // Если уже экипировано это же оружие
        if (player.currentWeaponIndex == id)
        {
            player.ShowMessage("Already equipped.");
            return;
        }

        if (player.strength > attackDamage)
        {
            player.ShowMessage("Current weapon is better.");
            return;
        }

        // Удаляем текущее оружие, если оно не стартовое, не кирка и не молот
        if (player.currentWeaponIndex != 0 && player.currentWeaponIndex != 7 && player.currentWeaponIndex != 8)
        {
            Item prevItemData = player.inventory.GetItemById(player.currentWeaponIndex);
            if (prevItemData != null)
            {
                ItemInstance prevWeapon = new ItemInstance
                {
                    itemData = prevItemData,
                    amount = 1
                };

                int remain = player.inventory.addItems(prevWeapon, 1);
                if (remain > 0)
                    player.inventory.DropItem(prevWeapon);
            }
        }

        // Присваиваем UI и характеристики
        player.strength = attackDamage;
        player.weaponSlotIcon.sprite = this.icon;
        player.weaponSlotIcon.enabled = true;
        player.ActualWeaponID = id;
        player.lastCombatWeaponID = id;
        player.SwapWeapon(id);

        //player.inventory.RemoveItem(itemInstance);
        player.inventory.onInventoryChanged.Invoke();

        Debug.Log($"Weapon {weaponName} equipped with {attackDamage} damage.");
    }
}

[CreateAssetMenu(fileName = "New Boots", menuName = "Inventory/Boots")]
public class Boots : Item
{
    public string bootsName;
    //public Sprite icon;
    public float speedBonus;

    public override void Use(ItemInstance itemInstance)
    {
        MovementInput player = FindObjectOfType<MovementInput>();

        // Если уже надеты такие же ботинки
        if (player.currentBootsIndex == id)
        {
            player.ShowMessage("Already equipped.");
            return;
        }

        // Получаем текущие надетые ботинки
        Item currentBootsItem = player.inventory.GetItemById(player.currentBootsIndex);
        if (currentBootsItem is Boots currentBoots)
        {
            if (player.speedBonus > this.speedBonus)
            {
                player.ShowMessage("Current boots are better.");
                return;
            }

            if (player.speedBonus == this.speedBonus)
            {
                player.ShowMessage("Already equipped.");
                return;
            }

            // Возвращаем текущие ботинки в инвентарь
            ItemInstance returnItem = new ItemInstance
            {
                itemData = currentBoots,
                amount = 1
            };

            int remaining = player.inventory.addItems(returnItem, 1);
            if (remaining > 0)
            {
                player.inventory.DropItem(returnItem);
            }
        }

        // Устанавливаем новые ботинки
        player.currentBootsIndex = id;
        player.speedBonus = speedBonus;
        player.bootsSlotIcon.sprite = icon;
        player.bootsSlotIcon.enabled = true;

        player.ShowMessage($"Equipped {bootsName}");

        // Удаляем ботинки из инвентаря
        for (int s = 0; s < player.inventory.slots.Count; s++)
        {
            if (player.inventory.slots[s].item.itemData.id == id)
            {
                player.inventory.slots[s].amount--;

                if (player.inventory.slots[s].amount <= 0)
                {
                    player.inventory.slots.RemoveAt(s);
                }

                player.inventory.onInventoryChanged.Invoke();
                break;
            }
        }

        player.inventory.onInventoryChanged.Invoke(); // Обновляем UI
    }
}

[CreateAssetMenu(fileName = "New Potion", menuName = "Inventory/Potion")]
public class Potion : Item
{
    public int healingAmount; // Количество здоровья, которое восстанавливает зелье
    public bool UltraPotion;

    public override void Use(ItemInstance itemInstance)
    {
        MovementInput player = FindObjectOfType<MovementInput>();

        // Логика использования зелья
        if (UltraPotion)
        {
            player.currentHealth = player.maxHealth;
            player.Healing(healingAmount);
        }
        else player.Healing(healingAmount);
        Debug.Log("Drinking potion " + itemName + " heals for: " + healingAmount);

        // Удаляем зелье из инвентаря через itemInstance
        player.inventory.RemoveItem(itemInstance);
        player.inventory.onInventoryChanged.Invoke();
    }
}

[CreateAssetMenu(fileName = "New Book", menuName = "Inventory/Book")]
public class Book : Item
{
    public int xpAmount;
    public bool UltraBook;
    float savexp;

    public override void Use(ItemInstance itemInstance)
    {
        MovementInput player = FindObjectOfType<MovementInput>();

        if (UltraBook)
        {
            savexp = player.currentXP;
            player.LevelUp();
            player.currentXP = savexp;
        }
        else player.GainXP(xpAmount);
        // Логика использования зелья


        player.inventory.RemoveItem(itemInstance);
        player.inventory.onInventoryChanged.Invoke();
    }
}

[CreateAssetMenu(fileName = "New Helmet", menuName = "Inventory/Helmet")]
public class Helmet : Item
{
    public GameObject model; // 3D модель шлема, если отличается от prefab

    public override void Use(ItemInstance itemInstance)
    {
        // Попытка найти голову игрока
        Transform head = GameObject.FindGameObjectWithTag("Head")?.transform;

        if (head == null)
        {
            Debug.LogWarning("Голова игрока не найдена (тег 'Head')");
            return;
        }

        // Удалить старый шлем, если есть
        foreach (Transform child in head)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Надеть новый шлем
        if (prefab != null)
        {
            GameObject helmetInstance = Instantiate(prefab, head);
            helmetInstance.transform.localPosition = Vector3.zero;
            helmetInstance.transform.localRotation = Quaternion.identity;

            // Сохранение выбора
            PlayerPrefs.SetInt("EquippedHelmetID", id);
            PlayerPrefs.Save();

            Debug.Log($"Шлем {itemName} надет.");
        }
        else
        {
            Debug.LogWarning("У шлема отсутствует prefab.");
        }
    }
}