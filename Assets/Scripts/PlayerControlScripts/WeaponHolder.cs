using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour {

    public weaponScript myWeapon;
    public GameObject hand;

	public void Use(bool input)
    {
        if(input && myWeapon)
        {
            myWeapon.FireShot();
        }
    }
}
