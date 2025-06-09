using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public Quest currentQuest;
    public Text questProgressText; // UI для прогресса квеста

    public MovementInput player;

    private void Start()
    {
        // Пример задания
        //AssignQuest(new Quest("Collect Wood", "Gather 20 pieces of wood.", 50, 100, 20));
    }

    public void AssignQuest(Quest quest)
    {
        currentQuest = quest;
        UpdateQuestUI();
    }

    public void UpdateQuestProgress(int amount)
    {
        if (currentQuest != null && !currentQuest.isCompleted)
        {
            currentQuest.UpdateProgress(amount);
            UpdateQuestUI();

            if (currentQuest.isCompleted)
            {
                CompleteQuest();
            }
        }
    }

    private void CompleteQuest()
    {
        Debug.Log("Quest Completed!");
        player.AddMoney(currentQuest.moneyReward);
        player.currentXP += currentQuest.xpReward;
        player.LevelUp();
        player.UpdateUI(); // Обновляем начальные значения UI
        player.UpdateXP();
        
        currentQuest = null;
    }

    private void UpdateQuestUI()
    {
        questProgressText.text = currentQuest.questName + ": " + currentQuest.currentAmount + "/" + currentQuest.targetAmount;
    }
}