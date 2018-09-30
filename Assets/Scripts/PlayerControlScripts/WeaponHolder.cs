using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour {

    public weaponScript equippedWeapon;
    public bool letEquipWeapons = true;
    public Transform hand;

	public void Use(bool input)
    {
        if(input && equippedWeapon)
        {
            equippedWeapon.FireShot();
        }
    }

    public void Equip(weaponScript item)
    {
        if(!letEquipWeapons) { return; }

        if (equippedWeapon)
        {
            if(equippedWeapon.rb) { equippedWeapon.rb.isKinematic = false; }
            if(equippedWeapon.col) { equippedWeapon.col.enabled = true; }

            equippedWeapon.transform.parent = null;
            equippedWeapon = null;
        }

        if(item)
        {
            if (equippedWeapon.rb) { equippedWeapon.rb.isKinematic = true; }
            if (equippedWeapon.col) { equippedWeapon.col.enabled = false; }

            item.transform.parent = hand;
            equippedWeapon = item;
        }
    }
}
