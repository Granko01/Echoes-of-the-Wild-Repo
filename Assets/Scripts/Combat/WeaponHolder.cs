using UnityEngine;

// Attach to the Player root. Finds all WeaponBase children and activates the equipped one.
// PlayerController routes Attack/WeaponSkill inputs here.
[RequireComponent(typeof(PlayerController))]
public class WeaponHolder : MonoBehaviour
{
    [SerializeField] private int _equippedIndex = 0;

    private WeaponBase[]    _weapons;
    private PlayerController _pc;

    public WeaponBase Current => (_weapons != null && _equippedIndex < _weapons.Length)
                                  ? _weapons[_equippedIndex]
                                  : null;

    private void Awake()
    {
        _pc      = GetComponent<PlayerController>();
        _weapons = GetComponentsInChildren<WeaponBase>(includeInactive: true);

        for (int i = 0; i < _weapons.Length; i++)
        {
            if (i == _equippedIndex) _weapons[i].OnEquip();
            else                     _weapons[i].OnUnequip();
        }
    }

    public void OnAttack()
    {
        Current?.TryAttack(_pc.FacingDir);
    }

    public void OnWeaponSkill()
    {
        Current?.TryWeaponSkill(_pc.FacingDir);
    }

    public void Equip(int index)
    {
        if (_weapons == null || index < 0 || index >= _weapons.Length) return;
        Current?.OnUnequip();
        _equippedIndex = index;
        Current?.OnEquip();
    }

    public void EquipByWeaponId(string id)
    {
        for (int i = 0; i < _weapons.Length; i++)
        {
            if (_weapons[i].Data != null && _weapons[i].Data.weaponId == id)
            {
                Equip(i);
                return;
            }
        }
    }
}
