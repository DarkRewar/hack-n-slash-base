using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;

public class BossBehaviour : EnemyBehaviour
{
    [Header("Boss Settings")]
    public EnemyBehaviour EnemyToSpawn;
    public float SpawnDelay = 10;
    public int EnemiesForEachSpawn = 1;
    public float SpawnRadius = 5;

    private BossStates _state;
    private float _cooldownSpawn = 0;

    public enum BossStates
    {
        Idle,
        Attacking
    }

    protected override void Update()
    {
        base.Update();

        if(_state != BossStates.Idle)
        {
            if(_cooldownSpawn <= 0)
            {
                SpawnEnemies();
            }
            else
            {
                _cooldownSpawn -= Time.deltaTime;
            }
        }
    }

    protected override void Attack()
    {
        base.Attack();

        _state = BossStates.Attacking;
    }

    private void SpawnEnemies()
    {
        _cooldownSpawn = SpawnDelay;

        for(int i = 0; i < EnemiesForEachSpawn; ++i)
        {
            Vector2 rand = UnityEngine.Random.insideUnitCircle * SpawnRadius * 2;
            Vector3 pos = new Vector3(rand.x, 0, rand.y) + transform.position;

            if(NavMesh.FindClosestEdge(pos, out NavMeshHit hit, LayerMask.NameToLayer("Ground")))
            {
                pos = hit.position;
            }

            EnemyBehaviour enemy = Instantiate(EnemyToSpawn, pos, Quaternion.identity);
            enemy.SetTarget(_player);
        }
    }
}
