using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    [SerializeReference] public Item itemData;

    public int amount; // Количество предметов в стопке

    public void use()
    {
        itemData.Use(this);
        //DecreaseAmount(1); // Уменьшаем количество при использовании
    }
}