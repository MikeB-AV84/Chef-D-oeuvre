using UnityEngine;
using System.Collections;

public class BossFireController : MonoBehaviour
{
    public float fireInterval = 5f;
    private Boss boss;

    void Start()
    {
        boss = GetComponent<Boss>();
        if (boss == null)
        {
            Debug.LogError("BossFireController: Boss component not found on this GameObject.");
            enabled = false; // Disable script if boss component is missing
            return;
        }
        StartCoroutine(FireRoutine());
    }

    IEnumerator FireRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireInterval);
            if (boss != null) // Check if boss still exists
            {
                boss.StartMissileAttack(); // MODIFIED LINE
            }
            else
            {
                yield break; // Stop coroutine if boss is destroyed
            }
        }
    }
}