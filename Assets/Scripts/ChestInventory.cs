using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChestInventory : MonoBehaviour
{
    public List<ItemInstance> chestSlots = new List<ItemInstance>(); // Слоты сундука
    public Image[] chestSlotIcons; // Иконки для отображения слотов сундука
    public Inventory playerInventory; // Ссылка на инвентарь игрока

    private void Start()
    {
        //playerInventory = FindObjectOfType<Inventory>(); // Находим инвентарь игрока
        InitializeChestSlots(); // Инициализируем слоты сундука
    }

    private void InitializeChestSlots()
    {
        for (int i = 0; i < chestSlotIcons.Length; i++)
        {
            chestSlots.Add(null); // Изначально все слоты пустые
            chestSlotIcons[i].enabled = false; // Иконки выключены
        }
    }

    public bool AddToChest(ItemInstance itemInstance)
    {
        for (int i = 0; i < chestSlots.Count; i++)
        {
            if (chestSlots[i] == null) // Если слот пуст
            {
                chestSlots[i] = itemInstance;
                chestSlotIcons[i].sprite = chestSlots[i].itemData.icon;
                chestSlotIcons[i].enabled = true;
                Debug.Log($"Item {itemInstance.itemData.itemName} added to chest in slot {i}.");
                return true;
            }
        }
        Debug.LogWarning("Chest is full!");
        return false; // Если сундук полон
    }

    public void MoveToPlayerInventory(int chestSlotIndex)
    {
        // Проверяем, что индекс сундука валиден
        if (chestSlotIndex >= 0 && chestSlotIndex < chestSlots.Count && chestSlots[chestSlotIndex] != null)
        {
            // Получаем предмет из сундука
            ItemInstance itemToMove = chestSlots[chestSlotIndex];

            // Перемещаем предмет в инвентарь
            int remainingAmount = playerInventory.addItems(itemToMove, itemToMove.amount);

            if (remainingAmount == 0)
            {
                // Если все предметы перемещены, очищаем слот сундука
                chestSlots[chestSlotIndex] = null;
                Debug.Log($"Все предметы {itemToMove.itemData.itemName} перемещены из сундука в инвентарь.");
            }
            else
            {
                // Если не все предметы поместились, обновляем количество в сундуке
                itemToMove.amount = remainingAmount;
                Debug.Log($"Часть предметов {itemToMove.itemData.itemName} перемещена в инвентарь. Осталось в сундуке: {remainingAmount}.");
            }

            chestSlotIcons[chestSlotIndex].enabled = false;
            playerInventory.onInventoryChanged.Invoke(); // Обновляем UI инвентаря игрока
        }
        else
        {
            Debug.LogWarning("Некорректный слот сундука или слот пуст.");
        }
    }

    public void MoveToChestFromInventory(int playerInventoryIndex)
    {
        Debug.Log(playerInventoryIndex + "  1  " + playerInventory.items.Count);
        if (playerInventoryIndex >= 0 && playerInventoryIndex < playerInventory.items.Count)
        {
            Debug.Log("1");
            ItemInstance itemToMove = playerInventory.getItem(playerInventoryIndex);

            if (itemToMove != null)
            {
                Debug.Log("2");

                bool added = AddToChest(itemToMove);

                if (added)
                {
                    Debug.Log("3");

                    playerInventory.RemoveItem(itemToMove);
                    Debug.Log($"Item {itemToMove.itemData.itemName} moved from player inventory to chest.");
                }
                else
                {
                    Debug.LogWarning("Failed to add item to chest. Chest might be full.");
                }
            }
        }
    }
}