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

    private int point = 0;
    private WaitForSeconds wait;

    private static GameObject s_prefab;
    private static List<Transform> s_spawnPos;
    private static int s_point = 0;

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
            if (point >= spawnPos.Count)
            {
                point = 0;
            }

            Spawn(spawnPos[point].position);

            point++;
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
            if (s_point >= s_spawnPos.Count)
            {
                s_point = 0;
            }

            var newEnemy = ObjectPool.Spawn(s_prefab, s_spawnPos[s_point].position);

            NetworkServer.Spawn(newEnemy);

            s_point++;
        }
    }
}
