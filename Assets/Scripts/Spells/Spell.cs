using UnityEngine;
using System.Collections;
using UnityEngine.VFX;

public class Spell : ScriptableObject
{
    #region MEMBERS

    public Sprite Icon;

    public string Title;

    [TextArea(4, 10)]
    public string Description;

    public uint Cost = 10;

    public uint Damages = 10;

    public float Range = 10;

    public float Cooldown = 1;

    public GameObject Prefab;

    #endregion

    #region ACCESSEURS

    public float CooldownLeft => Mathf.Clamp(_cooldownLeft, 0, Cooldown);

    public bool CanCast => _cooldownLeft <= 0;

    #endregion

    #region PRIVATES

    private float _cooldownLeft = 0;

    #endregion

    public void UpdateCooldown()
    {
        if(_cooldownLeft > 0)
        {
            _cooldownLeft -= Time.deltaTime;
        }
    }

    public virtual void Cast(PlayerBehaviour player, Vector3 impact)
    {
        _cooldownLeft = Cooldown;

        Vector3 newDir = impact;
        newDir.y = player.transform.position.y;
        Quaternion newRotation = Quaternion.LookRotation(newDir - player.transform.position);

        GameObject spell = Instantiate(Prefab, player.transform.position + Vector3.up * 0.5, newRotation);
        if(spell.TryGetComponent(out SpellHitbox hitbox))
        {
            hitbox.Player = player;
            hitbox.Spell = this;
        }

        Destroy(spell, 3);
    }
}
