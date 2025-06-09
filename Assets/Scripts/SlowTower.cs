using System.Collections.Generic;
using UnityEngine;

public class SlowTower : MonoBehaviour
{
    public int towerLevel = 1;

    private float slowRadius = 4f;
    private float slowAmount = 0.15f;
    public LayerMask enemyLayer;
    public float checkInterval = 0.2f;
    public Color slowEffectColor = Color.cyan;

    private Dictionary<EnemyAI, float> slowedEnemies = new Dictionary<EnemyAI, float>();

    void Start()
    {
        InvokeRepeating(nameof(CheckForEnemies), 0f, checkInterval);
    }

    void Update()
    {
        slowAmount = 1f - (0.1f * (towerLevel + 2f));
        slowRadius = towerLevel + 3f;
    }

    void CheckForEnemies()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, slowRadius, enemyLayer);

        HashSet<EnemyAI> currentEnemies = new HashSet<EnemyAI>();

        foreach (Collider enemyCollider in enemiesInRange)
        {
            EnemyAI enemyAI = enemyCollider.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                if (!slowedEnemies.ContainsKey(enemyAI))
                {
                    ApplySlowEffect(enemyAI);
                }
                currentEnemies.Add(enemyAI);
            }
        }

        // Убираем эффект с врагов, которые вышли из радиуса
        List<EnemyAI> toRemove = new List<EnemyAI>();
        foreach (var kvp in slowedEnemies)
        {
            if (!currentEnemies.Contains(kvp.Key))
            {
                RemoveSlowEffect(kvp.Key);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var enemy in toRemove)
        {
            slowedEnemies.Remove(enemy);
        }
    }

    void ApplySlowEffect(EnemyAI enemy)
    {
        if (enemy != null && !slowedEnemies.ContainsKey(enemy))
        {
            enemy.ModifySpeed(slowAmount);
            slowedEnemies.Add(enemy, Time.time); // Важно!
                                                 //SetEnemyColor(enemy, slowEffectColor);
        }
    }

    void RemoveSlowEffect(EnemyAI enemy)
    {
        if (enemy != null && slowedEnemies.ContainsKey(enemy))
        {
            enemy.ModifySpeed(1f);
            //SetEnemyColor(enemy, enemy.OriginalColor);
        }
    }

    void SetEnemyColor(EnemyAI enemy, Color color)
    {
        Renderer enemyRenderer = enemy.GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = color;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slowRadius);
    }
}