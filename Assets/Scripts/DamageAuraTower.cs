using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DamageAuraTower : MonoBehaviour
{
    public int towerLevel = 1;

    private float damageRadius = 6f;           // Радиус действия урона
    private float damageAmount = 1f;           // Урон за тик
    private float damageInterval = 0.75f;       // Интервал между ударами
    public LayerMask enemyLayer;              // Слой врагов
    public Color auraColor = Color.red;       // Цвет отображения

    private LineRenderer lineRenderer;
    private const int segments = 50;

    void Start()
    {
        InvokeRepeating(nameof(DealDamageToEnemies), 0f, damageInterval);
        SetupLineRenderer();
        //DrawCircle();
    }

    void Update()
    {
        damageAmount = 1f * towerLevel;
    }

    void DealDamageToEnemies()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, damageRadius, enemyLayer);

        foreach (Collider enemyCollider in enemiesInRange)
        {
            EnemyAI enemy = enemyCollider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damageAmount);
            }
        }
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = auraColor;
        lineRenderer.endColor = auraColor;
    }

    void DrawCircle()
    {
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments);
            float rad = Mathf.Deg2Rad * angle;
            Vector3 point = transform.position + new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * damageRadius;
            lineRenderer.SetPosition(i, point);
        }
    }
}