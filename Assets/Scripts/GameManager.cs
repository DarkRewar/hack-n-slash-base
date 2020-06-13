using UnityEngine;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    #region SINGLETON

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;    
    }

    #endregion

    public static event Action<EnemyBehaviour> OnEnemyDie;

    public PlayerBehaviour Player { get; private set; }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    internal void SetPlayer(PlayerBehaviour player)
    {
        Player = player;
    }

    internal static void KillEnemy(EnemyBehaviour enemyBehaviour)
    {
        OnEnemyDie?.Invoke(enemyBehaviour);
    }
}
