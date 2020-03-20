using UnityEngine;
using Mirror;

[RequireComponent(typeof(BoxCollider2D))]
public class Enemy : NetworkBehaviour
{
    [Header("Health & Damage")]
    [SyncVar] public float health = 10f;
    public float damage = 2f;

    [Header("Movement Settings")]
    public float speed = 5f;

    private bool isDead = false;

    private Player[] players;

    private void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            isDead = true;
        }
    }

    private void IsDead()
    {
        if (isDead)
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
