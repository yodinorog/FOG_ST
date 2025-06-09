using UnityEngine;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Для использования TMP_Text
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;                // Ссылка на инвентарь
    [SerializeField] List<Image> icons = new List<Image>();        // Список изображений для отображения иконок предметов
    [SerializeField] List<Text> amounts = new List<Text>(); // Список текстовых полей для отображения количества предметов

    public void UpdateUI()  // Обновление визуального представления инвентаря
    {
        // Перебор всех слотов инвентаря
        for (int i = 0; i < inventory.getSize(); i++)  // Перебор непустых ячеек
        {
            icons[i].color = new Color(1, 1, 1, 1);  // Устанавливаем цвет иконки (полностью видимый)
            icons[i].sprite = inventory.getItem(i).itemData.icon;  // Устанавливаем изображение иконки
            icons[i].gameObject.SetActive(true);  
            amounts[i].text = (inventory.getAmount(i) > 1) ? inventory.getAmount(i).ToString() : "";  // Если больше 1, показываем количество
        }

        // Перебор пустых ячеек
        for (int i = inventory.getSize(); i < icons.Count; i++)
        {
            icons[i].color = new Color(1, 1, 1, 0);  // Устанавливаем прозрачный цвет для пустых иконок
            icons[i].sprite = null;  // Убираем изображение
            amounts[i].text = "";  // Очищаем текст
        }
    }
}