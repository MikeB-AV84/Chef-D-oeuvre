using UnityEngine;
using System.Collections;

public class BossFireController : MonoBehaviour
{
    public float fireInterval = 5f;
    private Boss boss;

    void Start()
    {
        boss = GetComponent<Boss>();
        StartCoroutine(FireRoutine());
    }

    IEnumerator FireRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireInterval);
            StartCoroutine(boss.FireMissiles());
        }
    }
}
