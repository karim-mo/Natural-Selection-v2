using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Timeline;

public class ShootingHandling : MonoBehaviour
{
    [Header("Weapon Attach Locations")]
    public Transform rightHand;
    public Transform Spine;


    [Header("Bullet Prefabs")]
    public GameObject Bullet;
    public GameObject bulletHole;


    public Camera cam;

    private PlayerController player;
    
    private float nextFire;
    private float nextRecoil;
    private float recoilTimer;
    private Vector3 origin;

    [HideInInspector]
    public List<WeaponDB.Weapons> currWeapons;
    [HideInInspector]
    public WeaponDB.Weapons currWeapon;
    [HideInInspector]
    public WeaponDB.Weapons prevWeapon;
    [HideInInspector]
    public bool inShootCD;
    [HideInInspector]
    public Vector3 aimOffset;

    void Start()
    {
        player = GetComponent<PlayerController>();
        aimOffset = Vector3.zero;
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

        Recoil(player.targetPos);
    }

    public void Shoot(Vector3 target, float range)
    {
        if (player.fire && Time.time >= nextFire && !player.isReloading && currWeapon.currBullets > 0)
        {
            nextRecoil = Time.time + currWeapon.recoilRate;
            nextFire = Time.time + currWeapon.fireRate;
            Transform weapTransform = currWeapon.go.transform;
            Transform muzzle = weapTransform.GetChild(weapTransform.childCount - 1);
            muzzle.LookAt(target + aimOffset, Vector3.up);
            Ray ray = new Ray(muzzle.position, muzzle.transform.forward);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, range);
            Debug.DrawRay(ray.origin, ray.direction * range, Color.cyan);
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                GameObject go = Instantiate(bulletHole, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                Destroy(go, 10f);
                //Destroy(hit.collider.gameObject);
            }
            currWeapon.currBullets--;
        }
    }

    public void Attach(GameObject weapon, WeaponDB.Weapons refWeapon)
    {
        nextFire = 0;
        nextRecoil = 0;
        recoilTimer = 0;
        aimOffset = Vector3.zero;
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

    public void Recoil(Vector3 target)
    {
        if (nextRecoil > Time.time)
        {
            recoilTimer += Time.deltaTime;
            recoilTimer = Mathf.Clamp(recoilTimer, 0, currWeapon.recoilSpeed);
            float curveTime = recoilTimer / currWeapon.recoilSpeed;


            //All recoils for this bullet
            Vector3 allRecoils = Vector3.zero;
            for(int i = 0; i < currWeapon.recoils.Length; i++)
            {
                if (recoilTimer == currWeapon.recoilSpeed) continue;

                allRecoils += transform.TransformDirection(currWeapon.recoils[i].direction) * currWeapon.recoils[i].curve.Evaluate(curveTime);
            }

            aimOffset = Vector3.Lerp(aimOffset, aimOffset + allRecoils, currWeapon.recoilStrength * Time.deltaTime);
            Vector3 diff = target - (target + aimOffset);
            //Debug.Log(diff);
            //player.camBase.transform.LookAt(target, Vector3.up);
            
            //player.cam.transform.DOShakeRotation(target + aimOffset, 0.1f);
            //player.cam.transform.DOMove
            //player.camBase.mouseX += 20;
            //player.cam.DOShakePosition(0.01f, aimOffset + allRecoils, 0, 0, false);
            //player.cam.DOShakePosition(1f, (target + aimOffset) / 2f, 2, 0, false);
            //player.cam.transform.localPosition = 
            //Vector3.Lerp(player.cam.transform.localPosition, (player.cam.transform.localPosition + aimOffset) / 2f , 30 * Time.deltaTime);
        }
        else
        {
            //player.cam.transform.LookAt(new Vector3(0, transform.position.y, 0));
            //player.cam.transform.rotation = Quaternion.identity;
            //player.cam.transform.LookAt(Quaternion.identity);
            //player.cam.transform.LookAt()
            //if (recoilTimer > currWeapon.recoilSpeed / 2f)
            //{
            //    recoilTimer -= Time.deltaTime * 2;
            //}
            //else
            //{
            //    recoilTimer -= Time.deltaTime;
            //}
            ////recoilTimer = Mathf.Clamp(recoilTimer, recoilTimer, 0);
            //if (recoilTimer < 0) recoilTimer = 0;

            //if (recoilTimer <= 0)
            //{
            //    aimOffset = Vector3.zero;
            //}
            recoilTimer = 0;
            aimOffset = Vector3.zero;
        }
    }

}
