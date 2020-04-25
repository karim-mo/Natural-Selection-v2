using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShootingHandling : MonoBehaviour
{
    public Transform rightHand;
    public Transform Spine;


    private PlayerController player;
    [HideInInspector]
    public List<WeaponDB.Weapons> currWeapons;
    [HideInInspector]
    public WeaponDB.Weapons currWeapon;
    [HideInInspector]
    public WeaponDB.Weapons prevWeapon;

    void Start()
    {
        player = GetComponent<PlayerController>();
        currWeapons = new List<WeaponDB.Weapons>();
        currWeapons.Add(WeaponDB.instance.GetWeaponByName("M4A1"));
        currWeapons.Add(WeaponDB.instance.GetWeaponByName("AK47"));
        currWeapon = currWeapons[0];
        currWeapon.go = Instantiate(currWeapon.prefab);
        Detach(currWeapon.go, currWeapon);
        currWeapon = null;
    }

    void Update()
    {
        
    }

    public void Shoot()
    {

    }

    public void Attach(GameObject weapon, WeaponDB.Weapons refWeapon)
    {
        weapon.transform.SetParent(rightHand, false);
        weapon.transform.localPosition = refWeapon.handPos;
        weapon.transform.localEulerAngles = refWeapon.handRot;
    }

    public void Detach(GameObject weapon, WeaponDB.Weapons refWeapon)
    {
        weapon.transform.SetParent(Spine, false);
        weapon.transform.localPosition = refWeapon.spinePos;
        weapon.transform.localEulerAngles = refWeapon.spineRot;
    }

}
