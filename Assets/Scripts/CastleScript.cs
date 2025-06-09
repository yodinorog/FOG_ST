using UnityEngine;
using System.Collections;

public class CastleScript : MonoBehaviour
{
    public int HP = 100;
    public int maxHP = 500;
    public int regenAmount = 1;

    private MovementInput player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<MovementInput>();
            if (player != null)
            {
                player.UpdateCastleUI(HP);
            }
        }

        if (player == null)
            Debug.LogWarning("CastleScript: Не найден MovementInput у игрока.");

        // Запускаем регенерацию
        StartCoroutine(RegenerateHP());
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        HP = Mathf.Max(0, HP);

        if (player != null)
        {
            player.UpdateCastleUI(HP);
            player.CastleTakeDamageUI();
        }
    }

    private IEnumerator RegenerateHP()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.5f);

            if (HP > 0 && HP < maxHP)
            {
                HP += regenAmount;
                HP = Mathf.Min(HP, maxHP);

                if (player != null)
                    player.UpdateCastleUI(HP);
            }
        }
    }
}