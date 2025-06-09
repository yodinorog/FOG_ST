using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MagicTower : MonoBehaviour
{
    public int towerLevel = 1;

    public float detectionRadius = 10f;
    private float freezeDuration = 3f;
    public float damageInterval = 0.2f;
    private int damagePerTick = 1;

    private float fireCooldown = 9f;
    private float cooldownTimer = 0f;

    public LayerMask enemyLayer;
    public ParticleSystem freezeEffectPrefab;
    public int maxAffectedEnemiesPerZone = 10;

    public float zoneRadius = 2f;
    public float zoneMinDistance = 3f;
    public int maxPlacementAttempts = 500;

    private List<Vector3> placedZones = new List<Vector3>();

    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        damageInterval = 0.3f / Mathf.Max(1, 2 * (towerLevel - 1));
        if (cooldownTimer <= 0f)
        {
            cooldownTimer = fireCooldown;
            placedZones.Clear();
            TriggerZones(towerLevel);
        }
    }

    void TriggerZones(int numberOfZones)
    {
        int zonesPlaced = 0;
        int attempts = 0;

        while (zonesPlaced < numberOfZones && attempts < maxPlacementAttempts)
        {
            Vector3 randomOffset = Random.insideUnitSphere * detectionRadius;
            randomOffset.y = 0;
            Vector3 checkPos = transform.position + randomOffset + Vector3.up * 10f;

            if (Physics.Raycast(checkPos, Vector3.down, out RaycastHit hit, 20f))
            {
                if (IsFarFromOtherZones(hit.point) &&
                    NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
                {
                    StartCoroutine(FreezeZoneCoroutine(navHit.position));
                    placedZones.Add(navHit.position);
                    zonesPlaced++;
                }
            }

            attempts++;
        }
    }

    bool IsFarFromOtherZones(Vector3 newPos)
    {
        foreach (var pos in placedZones)
        {
            if (Vector3.Distance(pos, newPos) < zoneMinDistance)
                return false;
        }
        return true;
    }

    IEnumerator FreezeZoneCoroutine(Vector3 zoneCenter)
    {
        if (freezeEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(freezeEffectPrefab, zoneCenter, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, freezeDuration + 1f);
        }

        float elapsed = 0f;
        Dictionary<EnemyAI, float> originalSpeeds = new Dictionary<EnemyAI, float>();

        while (elapsed < freezeDuration)
        {
            Collider[] hits = Physics.OverlapSphere(zoneCenter, zoneRadius, enemyLayer);
            int affectedCount = 0;

            foreach (var col in hits)
            {
                if (affectedCount >= maxAffectedEnemiesPerZone)
                    break;

                EnemyAI enemy = col.GetComponent<EnemyAI>();
                if (enemy != null && !enemy.isDead)
                {
                    if (!originalSpeeds.ContainsKey(enemy))
                    {
                        originalSpeeds[enemy] = enemy.moveSpeed;
                        enemy.magicSlow = -10f;
                    }

                    enemy.TakeDamage(damagePerTick);
                    affectedCount++;
                }
            }

            elapsed += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        // Восстановление скорости
        foreach (var pair in originalSpeeds)
        {
            if (pair.Key != null && !pair.Key.isDead)
                pair.Key.magicSlow = 0f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}