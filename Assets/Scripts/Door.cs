using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public List<EnemyBehaviour> EnemiesToCheck;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.OnEnemyDie += OnEnemyDie;
    }

    private void OnDestroy()
    {
        GameManager.OnEnemyDie -= OnEnemyDie;
    }

    private void OnEnemyDie(EnemyBehaviour obj)
    {
        EnemiesToCheck.Remove(obj);

        if (EnemiesToCheck.Count == 0)
        {
            GameManager.OnEnemyDie -= OnEnemyDie;
            StartCoroutine(OpenDoor());
        }
    }

    private IEnumerator OpenDoor()
    {
        float time = 0;
        while(time < 2)
        {
            transform.Translate(Vector3.down * Time.deltaTime * 3);
            time += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
