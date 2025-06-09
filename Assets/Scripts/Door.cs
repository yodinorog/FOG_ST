using UnityEngine;

public class Door : MonoBehaviour
{
    public int keysRequired = 1;
    public Item keyItem; // нужный тип ключа
    public bool isOpen = false;

    public void TryOpen(Inventory inventory)
    {
        if (isOpen) return;

        if (keyItem == null)
        {
            Debug.LogWarning("У двери не назначен ключ (keyItem)");
            return;
        }

        int keyCount = inventory.GetItemCount(keyItem.id);

        if (keyCount >= keysRequired)
        {
            isOpen = true;

            // Удаление нужного количества ключей
            int keysToRemove = keysRequired;
            for (int i = 0; i < inventory.slots.Count && keysToRemove > 0; i++)
            {
                var slot = inventory.slots[i];
                if (slot.item.itemData == keyItem)
                {
                    int removeAmount = Mathf.Min(slot.amount, keysToRemove);
                    slot.amount -= removeAmount;
                    keysToRemove -= removeAmount;

                    if (slot.amount <= 0)
                        inventory.slots.RemoveAt(i--);
                }
            }

            inventory.onInventoryChanged.Invoke();

            // Воспроизведение звука двери
            FindObjectOfType<AudioManager>().Play("Door");

            // Отключение объекта двери
            gameObject.SetActive(false);
        }
        else
        {
            FindObjectOfType<MovementInput>().ShowMessage("Not enough keys");
        }
    }
}