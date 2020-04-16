using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ObjectPool : NetworkBehaviour
{
    private const int DEFAULT_POOL_SIZE = 10;
    private const string DEFAULT_NAME = "Pooled";

    class Pool
    {
        int nextID = 0;
        Stack<GameObject> inactive;
        GameObject prefab;
        GameObject parent;

        public Pool(GameObject prefab, int initialQuantity)
        {
            this.prefab = prefab;
            parent = new GameObject(prefab.name + "'s");
            inactive = new Stack<GameObject>(initialQuantity);
        }

        public GameObject Spawn(Vector2 pos, string name = DEFAULT_NAME)
        {
            GameObject gameObject;

            if (inactive.Count == 0)
            {
                gameObject = (GameObject)Instantiate(prefab, pos, Quaternion.identity);
                gameObject.name = prefab.name + "(" + name + " " + (nextID++) + ")";
                gameObject.AddComponent<PoolMember>().myPool = this;
            }
            else
            {
                gameObject = inactive.Pop();

                if (gameObject == null)
                {
                    return Spawn(pos);
                }
            }

            gameObject.transform.parent = parent.transform;
            gameObject.transform.position = pos;
            gameObject.SetActive(true);
            return gameObject;
        }

        public void Despawn(GameObject gameObject)
        {
            gameObject.SetActive(false);
            NetworkServer.UnSpawn(gameObject);
            inactive.Push(gameObject);
        }
    }

    class PoolMember : MonoBehaviour
    {
        public Pool myPool;
    }

    static Dictionary<GameObject, Pool> pools;

    static void Init(GameObject prefab = null, int qty = DEFAULT_POOL_SIZE)
    {
        if (pools == null)
        {
            pools = new Dictionary<GameObject, Pool>();
        }

        if (prefab != null && pools.ContainsKey(prefab) == false)
        {
            pools[prefab] = new Pool(prefab, qty);
        }
    }

    static public void Preload(GameObject prefab, int qty = DEFAULT_POOL_SIZE, string name = DEFAULT_NAME)
    {
        Init(prefab, qty);

        GameObject[] gameObjects = new GameObject[qty];

        for (int i = 0; i < qty; i++)
        {
            gameObjects[i] = Spawn(prefab, Vector2.zero, name);
        }

        for (int i = 0; i < qty; i++)
        {
            Despawn(gameObjects[i]);
        }
    }

    static public GameObject Spawn(GameObject prefab, Vector2 pos, string name = DEFAULT_NAME)
    {
        Init(prefab);

        return pools[prefab].Spawn(pos, name);
    }

    static public void Despawn(GameObject gameObject)
    {
        PoolMember poolMember = gameObject.GetComponent<PoolMember>();

        if (poolMember == null)
        {
            Debug.Log("Objects '" + gameObject.name + "' wasn't spawned from a pool. Destroying it instead.");
            NetworkServer.Destroy(gameObject);
        }
        else
        {
            poolMember.myPool.Despawn(gameObject);
        }
    }
}
