using UnityEngine;

public class Quest
{
    public string questName;
    public string questDescription;
    public int xpReward;
    public int moneyReward;
    public bool isCompleted;
    public int targetAmount;
    public int currentAmount;

    public Quest(string name, string description, int xp, int money, int target)
    {
        questName = name;
        questDescription = description;
        xpReward = xp;
        moneyReward = money;
        targetAmount = target;
        currentAmount = 0;
        isCompleted = false;
    }

    public void UpdateProgress(int amount)
    {
        currentAmount += amount;
        if (currentAmount >= targetAmount)
        {
            isCompleted = true;
        }
    }
}