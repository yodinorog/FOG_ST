using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUse : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemInstance itemInstance;  // Экземпляр предмета
    public Image itemIcon;             // Иконка предмета в слоте
    private Image draggedItemIcon;     // Иконка для перетаскивания
    private Canvas canvas;             // Canvas для корректного отображения предмета
    public int slotIndex;              // Индекс слота
    private Inventory inventory;       // Ссылка на инвентарь
    private bool isPointerOver;        // Флаг для отслеживания, находится ли курсор над слотом

    private void Start()
    {
        inventory = GameObject.FindObjectOfType<Inventory>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetItem(ItemInstance newItem)
    {
        itemInstance = newItem;
        if (itemInstance != null)
        {
            itemIcon.sprite = itemInstance.itemData.icon;
            itemIcon.enabled = true;
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        itemInstance = null;
        itemIcon.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemInstance != null)
        {
            itemInstance.use();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (itemInstance != null && draggedItemIcon == null)
        {
            draggedItemIcon = new GameObject("DraggedItemIcon", typeof(Image)).GetComponent<Image>();
            draggedItemIcon.sprite = itemIcon.sprite;
            draggedItemIcon.transform.SetParent(canvas.transform, false);
            draggedItemIcon.raycastTarget = false;
        }

        if (draggedItemIcon != null)
        {
            draggedItemIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItemIcon != null)
        {
            Destroy(draggedItemIcon.gameObject);
        }

        InventorySlotUse targetSlot = eventData.pointerCurrentRaycast.gameObject?.GetComponent<InventorySlotUse>();

        Debug.Log(targetSlot);

        if (targetSlot = null)
        {
            DropItem();
        }
        else if (targetSlot != this)
        {
            SwapItem(targetSlot);
        }
    }

    private void DropItem()
    {
        if (itemInstance != null)
        {
            inventory.DropItem(itemInstance);
            ClearSlot();
            Debug.Log($"Предмет {itemInstance.itemData.itemName} выброшен.");
        }
    }

    private void SwapItem(InventorySlotUse targetSlot)
    {
        ItemInstance tempItem = targetSlot.itemInstance;
        targetSlot.SetItem(itemInstance);
        SetItem(tempItem);

        // Обновляем инвентарь после обмена
        inventory.UpdateSlot(slotIndex, targetSlot.itemInstance);
        inventory.UpdateSlot(targetSlot.slotIndex, itemInstance);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true; // Курсор вошел в слот
        Debug.Log($"Курсор вошел в слот с индексом {slotIndex}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false; // Курсор вышел из слота
        Debug.Log($"Курсор покинул слот с индексом {slotIndex}");
    }
}