using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;

public class Spawner : NetworkBehaviour
{
    [SerializeField] private NetworkIdentity prefab = null;
    [SerializeField] private float timeBetweenSpawn = 0f;
    [SerializeField] private int amountToSpawn = 0;

    public List<Transform> spawnPos;

    private int p = 0;
    private WaitForSeconds wait;

    private static GameObject s_prefab;
    private static List<Transform> s_spawnPos;
    private static int s_p = 0;

    public override void OnStartServer()
    {
        base.OnStartServer();
        s_prefab = prefab.gameObject;
        s_spawnPos = spawnPos;

        wait = new WaitForSeconds(timeBetweenSpawn);
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        for (int i = 0; i < amountToSpawn; i++)
        {
            if (p >= spawnPos.Count)
            {
                p = 0;
            }

            Spawn(spawnPos[p].position);

            p++;
            yield return wait;
        }
    }

    private void Spawn(Vector2 pos)
    {
        NetworkServer.Spawn(ObjectPool.Spawn(prefab.gameObject, pos));
    }

    static public void SpawnWhenDied(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (s_p >= s_spawnPos.Count)
            {
                s_p = 0;
            }

            var newEnemy = ObjectPool.Spawn(s_prefab, s_spawnPos[s_p].position);

            NetworkServer.Spawn(newEnemy);

            s_p++;
        }
    }
}
