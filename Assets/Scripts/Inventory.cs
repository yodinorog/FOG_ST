using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Добавляем UnityEvents
using System.Linq;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>(); // Список предметов в инвентаре
    [SerializeField] public List<InventorySlot> slots;
    public int maxSlots = 6;                   // Максимальное количество ячеек
    public UnityEvent onInventoryChanged;      // Событие для обновления интерфейса инвентаря
    public Transform dropPosition;             // Позиция для выбрасывания предметов

    private void Update()
    {
        // Проверяем нажатие кнопки U
        if (Input.GetKeyDown(KeyCode.U))
        {
            SwapSlots(0, 1); // Меняем местами 0-й и 1-й слоты
        }
    }

    public void SwapSlots(int index1, int index2)
    {
        // Проверяем, что индексы валидны
        if (index1 < 0 || index1 >= items.Count || index2 < 0 || index2 >= items.Count)
        {
            Debug.LogWarning("Индексы слотов вне диапазона!");
            return;
        }

        // Меняем местами предметы
        Item tempItem = items[index1];
        items[index1] = items[index2];
        items[index2] = tempItem;

        // Вызываем событие обновления интерфейса
        onInventoryChanged?.Invoke();

        Debug.Log($"Содержимое слотов {index1} и {index2} поменялись местами.");
    }


    public int addItems(ItemInstance item, int amount)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.item.itemData.id == item.itemData.id)
            {
                if (slot.amount < item.itemData.max_stack)
                {
                    if ((slot.amount + amount) > item.itemData.max_stack)
                    {
                        amount -= item.itemData.max_stack - slot.amount;
                        slot.amount = item.itemData.max_stack;
                        onInventoryChanged.Invoke();
                        continue;
                    }

                    slot.amount += amount;
                    onInventoryChanged.Invoke();
                    SyncItemsWithSlots(); // Обновляем items
                    return 0;
                }
            }
        }

        if (slots.Count >= maxSlots) return amount;

        while (amount > item.itemData.max_stack)
        {
            ItemInstance itm = new ItemInstance
            {
                itemData = item.itemData,
                amount = item.itemData.max_stack
            };

            slots.Add(new InventorySlot(itm, itm.amount));
            amount -= itm.amount;
            onInventoryChanged.Invoke();
            if (slots.Count >= maxSlots) return amount;
        }

        if (amount > 0)
        {
            ItemInstance itm = new ItemInstance
            {
                itemData = item.itemData,
                amount = amount
            };

            slots.Add(new InventorySlot(itm, itm.amount));
            onInventoryChanged.Invoke();
        }

        SyncItemsWithSlots(); // Обновляем items
        return 0;
    }

    private void SyncItemsWithSlots()
    {
        //items.Clear();
        //foreach (var slot in slots)
        //{
        //    items.Add(slot.item.itemData);
        //}
    }

    public bool Add(Item item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventory is full!");
            return false;
        }
        else
        {

            items.Add(item);
            Debug.Log("Item added to inventory: " + item.itemName);
        }
        
        if (onInventoryChanged != null)
        {
            Debug.Log("Updating UI");
            onInventoryChanged.Invoke();
        }

        return true;
    }

    public int getSize()
    {
        return slots.Count;
    }

    public int getAmount(int i)
    {
        return (i < slots.Count) ? slots[i].amount : 0;
    }

    public int GetAmount(ItemInstance itemInstance)
    {
        foreach (var slot in slots)
        {
            if (slot.item.itemData == itemInstance.itemData)
            {
                return slot.amount;
            }
        }
        return 0;
    }

    public ItemInstance getItem(int i)
    {
        return (i < slots.Count) ? slots[i].item : null;
    }

    public Item GetItemById(int id)
    {
        foreach (var slot in slots)
        {
            if (slot.item.itemData != null && slot.item.itemData.id == id)
                return slot.item.itemData;
        }
        return null;
    }

    public int GetItemCount(int id)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (slot.item.itemData != null && slot.item.itemData.id == id)
            {
                total += slot.amount;
            }
        }
        return total;
    }

    public void DropItem(ItemInstance itemInstance)
    {
        Transform playerTransform = transform;
        Vector3 dropPosition = playerTransform.position + playerTransform.forward * 1.5f + Vector3.up * 0.5f;

        GameObject droppedItem = Instantiate(itemInstance.itemData.prefab, dropPosition, Quaternion.identity);
        droppedItem.name = itemInstance.itemData.itemName;

        // Указываем, кто выбросил
        ItemContainer container = droppedItem.GetComponent<ItemContainer>();
        if (container != null)
        {
            container.droppedBy = this.gameObject;
            container.hasExitedDropper = false; // явно
        }

        RemoveItem(itemInstance);
        Debug.Log("Item " + itemInstance.itemData.itemName + " dropped at position: " + dropPosition);
    }

    public void RemoveItem(ItemInstance itemInstance)
    {
        for (int i = 0; i < slots.Count; i++)
        {
                if (slots[i].item.itemData == itemInstance.itemData)
                {
                    // Сравнение itemData по ссылке — достаточно, если предметы уникальны
                    slots[i].amount--;

                    Debug.Log($"[Inventory] Removed item: {itemInstance.itemData.itemName}, left: {slots[i].amount}");

                    if (slots[i].amount <= 0)
                    {
                        slots.RemoveAt(i);
                        Debug.Log($"[Inventory] Slot with {itemInstance.itemData.itemName} removed.");
                    }

                    onInventoryChanged.Invoke();
                    return; // Критично: выходим сразу, чтобы не удалять другие слоты
            }
        }
        

        Debug.LogWarning($"[Inventory] Item not found to remove: {itemInstance.itemData.itemName}");
    }

    public void UpdateSlot(int slotIndex, ItemInstance itemInstance)
    {
        if (slotIndex >= 0 && slotIndex < slots.Count)
        {
            slots[slotIndex].item = itemInstance;
            onInventoryChanged.Invoke();
        }
    }

    public int GetItemCountByName(string itemName)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (slot.item.itemData != null && slot.item.itemData.itemName == itemName)
            {
                total += slot.amount;
            }
        }
        return total;
    }

    public void RemoveItemsByName(string itemName, int amount)
    {
        for (int i = 0; i < slots.Count && amount > 0; i++)
        {
            var slot = slots[i];
            if (slot.item.itemData != null && slot.item.itemData.itemName == itemName)
            {
                int toRemove = Mathf.Min(slot.amount, amount);
                slot.amount -= toRemove;
                amount -= toRemove;

                if (slot.amount <= 0)
                {
                    slots.RemoveAt(i);
                    i--; // корректировка индекса после удаления
                }
            }
        }

        onInventoryChanged.Invoke();
    }
}