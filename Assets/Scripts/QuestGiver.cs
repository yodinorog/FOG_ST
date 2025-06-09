using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    public QuestManager questManager;
    public string[] resourceQuests; // Массив ресурсов (дерево, камень и т.д.)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (questManager.currentQuest == null)
            {
                // Пример генерации случайного квеста
                string questType = resourceQuests[Random.Range(0, resourceQuests.Length)];
                questManager.AssignQuest(new Quest("Collect " + questType, "Gather 10 " + questType, 100, 200, 10));
                Debug.Log("New Quest Assigned!");
            }
            else
            {
                Debug.Log("You already have a quest!");
            }
        }
    }
}