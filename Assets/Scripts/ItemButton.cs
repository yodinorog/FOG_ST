//using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    // Ссылка на Image внутри кнопки
    public Image itemImage;

    // Словарь или массив, в котором каждому идентификатору предмета соответствует свой спрайт
    public Sprite[] itemSprites;

    // Уникальный идентификатор кнопки
    public int buttonId;

    // Метод для изменения спрайта в зависимости от идентификатора предмета и кнопки
    public void SetItem(int itemId, int buttonId)
    {
        // Устанавливаем идентификатор кнопки (можно использовать для дальнейшей логики)
        this.buttonId = buttonId;

        // Проверяем, что идентификатор предмета находится в допустимом диапазоне
        if (itemId >= 0 && itemId < itemSprites.Length)
        {
            // Устанавливаем спрайт, соответствующий идентификатору предмета
            itemImage.sprite = itemSprites[itemId];
            Debug.Log($"Button ID: {buttonId} set with item ID: {itemId}");
        }
        else
        {
            Debug.LogError("Invalid item ID");
        }
    }
}