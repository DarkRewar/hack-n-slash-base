using UnityEngine;
using System.Collections;

public class SpellHitbox : MonoBehaviour
{
    public PlayerBehaviour Player;
    public Spell Spell;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Enemy") && collider.gameObject.TryGetComponent(out EnemyBehaviour enemy))
        {
            enemy.TakeDamages((int)Spell.Damages, Player);
        }
    }
}
