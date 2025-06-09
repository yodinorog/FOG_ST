using System.Collections;
using UnityEngine;

public class ResourceCooldown : MonoBehaviour
{
    public float cooldownTime = 600f;
    private float timer = 0f;
    public bool isOnCooldown = false;
    private Renderer rend;

    public bool IsAvailable => !isOnCooldown;
    public float TimeRemaining => Mathf.Max(0, cooldownTime - timer);

    private void Start()
    {
        rend = GetComponentInChildren<Renderer>(); // берём визуальную часть
    }

    public void StartCooldown()
    {
        if (!isOnCooldown)
        {
            isOnCooldown = true;
            timer = 0f;
            if (rend != null) rend.enabled = false;
            StartCoroutine(CooldownRoutine());
        }
    }

    IEnumerator CooldownRoutine()
    {
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isOnCooldown = false;
        if (rend != null) rend.enabled = true;
    }
}