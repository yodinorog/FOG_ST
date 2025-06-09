using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ForestUnit
{
    public GameObject prefab;     // Префаб моба
    public int damage;            // Урон
    public int health;            // Здоровье
    public int cost;              // Стоимость моба
    public string type;           // Тип моба (например, Easy, Medium, Hard, Boss)
    public string location;       // Локация моба (например, LeftForest, RightForest и т.д.)
}

public class ForestUnitDatabase : MonoBehaviour
{
    public List<ForestUnit> forestUnits = new List<ForestUnit>(); // Список всех лесных мобов

    // Метод для получения списка мобов, подходящих по локации и типу
    public List<ForestUnit> GetUnitsByCriteria(string type, string location)
    {
        List<ForestUnit> matchingUnits = new List<ForestUnit>();

        foreach (var unit in forestUnits)
        {
            if (unit.type == type && unit.location == location)
            {
                matchingUnits.Add(unit);
            }
        }

        return matchingUnits;
    }
}