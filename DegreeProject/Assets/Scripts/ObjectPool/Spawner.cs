using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;

public class Spawner : NetworkBehaviour
{
    [SerializeField] private NetworkIdentity prefab;
    [SerializeField] private float timeBetweenSpawn;
    [SerializeField] private int amountToSpawn;

    public List<Transform> spawnPos;
    
    private int p = 0;
    private WaitForSeconds wait;

    public override void OnStartServer()
    {
        base.OnStartServer();

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
}
