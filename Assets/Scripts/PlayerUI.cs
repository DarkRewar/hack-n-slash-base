using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Slider ManaBar;

    public Button ASpellBtn;
    public Button ZSpellBtn;
    public Button ESpellBtn;
    public Button RSpellBtn;

    private TMP_Text _aSpellCDText;
    private TMP_Text _zSpellCDText;
    private TMP_Text _eSpellCDText;
    private TMP_Text _rSpellCDText;

    private PlayerBehaviour _player;

    // Start is called before the first frame update
    void Start()
    {
        if (ASpellBtn) _aSpellCDText = ASpellBtn.GetComponentInChildren<TMP_Text>();
        if (ZSpellBtn) _zSpellCDText = ZSpellBtn.GetComponentInChildren<TMP_Text>();
        if (ESpellBtn) _eSpellCDText = ESpellBtn.GetComponentInChildren<TMP_Text>();
        if (RSpellBtn) _rSpellCDText = RSpellBtn.GetComponentInChildren<TMP_Text>();

        _player = GameManager.Instance.Player;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!_player)
        {
            _player = GameManager.Instance.Player;

            if (_player)
            {
                SetupSpell(_player.ASpell, ASpellBtn);
                SetupSpell(_player.ZSpell, ZSpellBtn);
                SetupSpell(_player.ESpell, ESpellBtn);
                SetupSpell(_player.RSpell, RSpellBtn);

                ManaBar.minValue = 0;
                ManaBar.maxValue = _player.MaxMana;
                ManaBar.value = _player.Mana;
            }

            return;
        }

        ManaBar.value = _player.Mana;

        UpdateSpellBtn(ASpellBtn, _player.ASpell, _aSpellCDText);
        UpdateSpellBtn(ZSpellBtn, _player.ZSpell, _zSpellCDText);
        UpdateSpellBtn(ESpellBtn, _player.ESpell, _eSpellCDText);
        UpdateSpellBtn(RSpellBtn, _player.RSpell, _rSpellCDText);
    }

    private void SetupSpell(Spell spell, Button spellBtn)
    {
        if(spell && spellBtn)
            spellBtn.image.sprite = spell.Icon;
    }

    private void UpdateSpellBtn(Button spellBtn, Spell spell, TMP_Text _spellCDText)
    {
        if (spellBtn && spell)
        {
            int cd = Mathf.CeilToInt(spell.CooldownLeft);
            if (cd > 0)
            {
                _spellCDText.gameObject.SetActive(true);
                _spellCDText.text = cd.ToString();
            }
            else
            {
                _spellCDText.gameObject.SetActive(false);
            }
        } 
        else if (spellBtn.gameObject.activeSelf)
        {
            spellBtn.gameObject.SetActive(false);
        }
    }
}
