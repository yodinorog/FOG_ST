using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventorySlot
{
    public ItemInstance item;  // Экземпляр предмета
    public int amount;         // Количество предметов в ячейке

    // Конструктор класса InventorySlot
    public InventorySlot(ItemInstance item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}