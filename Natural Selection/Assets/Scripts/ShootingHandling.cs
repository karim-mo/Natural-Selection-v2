using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class ShootingHandling : MonoBehaviour
{
    [Header("Weapon Attach Locations")]
    public Transform rightHand;
    public Transform Spine;


    [Header("Bullet Prefabs")]
    public GameObject Bullet;
    public GameObject bulletHole;


    private PlayerController player;
    private float nextFire;

    [HideInInspector]
    public List<WeaponDB.Weapons> currWeapons;
    [HideInInspector]
    public WeaponDB.Weapons currWeapon;
    [HideInInspector]
    public WeaponDB.Weapons prevWeapon;
    [HideInInspector]
    public bool inShootCD;

    void Start()
    {
        player = GetComponent<PlayerController>();
        currWeapons = new List<WeaponDB.Weapons>();
        currWeapons.Add(WeaponDB.instance.GetWeaponByName("M4A1"));
        currWeapons.Add(WeaponDB.instance.GetWeaponByName("AK47"));
        currWeapons[0].currBullets = currWeapons[0].bullets;
        currWeapons[0].currReserve = currWeapons[0].reserve;
        currWeapons[1].currBullets = currWeapons[1].bullets;
        currWeapons[1].currReserve = currWeapons[1].reserve;
        currWeapon = currWeapons[0];
        currWeapon.go = Instantiate(currWeapon.prefab);
        Detach(currWeapon.go, currWeapon);
        currWeapon = null;
    }

    void Update()
    {
        if (currWeapon == null) return;

        if(currWeapon.currBullets <= 0 && currWeapon.currReserve > 0)
        {
            player.m_Reload();
        }
    }

    public void Shoot(Vector3 target, float range)
    {
        if (player.fire && Time.time >= nextFire && !player.isReloading && currWeapon.currBullets > 0)
        {
            nextFire = Time.time + currWeapon.fireRate;
            Transform weapTransform = currWeapon.go.transform;
            Transform muzzle = weapTransform.GetChild(weapTransform.childCount - 1);
            muzzle.LookAt(target, Vector3.up);
            Ray ray = new Ray(muzzle.position, muzzle.transform.forward);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, range);
            Debug.DrawRay(ray.origin, ray.direction * range, Color.cyan);
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                GameObject go = Instantiate(bulletHole, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                Destroy(go, 10f);
                Destroy(hit.collider.gameObject);
            }
            currWeapon.currBullets--;
        }
    }

    public void Attach(GameObject weapon, WeaponDB.Weapons refWeapon)
    {
        nextFire = 0;
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

    public void Reload()
    {
        if (currWeapon.currReserve <= 0) return;
        int m_bullets = currWeapon.bullets - currWeapon.currBullets;
        int m_reserve = currWeapon.currReserve - m_bullets;

        if(m_reserve < 0)
        {
            currWeapon.currBullets += currWeapon.currReserve;
            currWeapon.currReserve -= currWeapon.currReserve;
            return;
        }

        currWeapon.currBullets += m_bullets;
        currWeapon.currReserve = m_reserve;

    }

}
