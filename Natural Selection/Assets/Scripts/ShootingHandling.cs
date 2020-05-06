using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Timeline;
using Photon.Pun;

public class ShootingHandling : MonoBehaviourPun
{
    [Header("Weapon Attach Locations")]
    public Transform rightHand;
    public Transform Spine;


    [Header("Bullet Prefabs")]
    public GameObject Bullet;
    public GameObject bulletHole;


    public Camera cam;

    private PlayerController player;
    private Animator anim;

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
        if (!photonView.IsMine) return;

        player = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
        aimOffset = Vector3.zero;
        currWeapons = new List<WeaponDB.Weapons>();
        currWeapons.Add(WeaponDB.instance.GetWeaponByName("M4A1"));
        currWeapons.Add(WeaponDB.instance.GetWeaponByName("AK47"));
        currWeapons[0].currBullets = currWeapons[0].bullets;
        currWeapons[0].currReserve = currWeapons[0].reserve;
        currWeapons[1].currBullets = currWeapons[1].bullets;
        currWeapons[1].currReserve = currWeapons[1].reserve;
        currWeapon = currWeapons[0];
        currWeapon.go = PhotonNetwork.Instantiate(currWeapon.prefab.name, currWeapon.prefab.transform.position, currWeapon.prefab.transform.rotation);
        Detach(currWeapon.go, currWeapon);
        currWeapon = null;
    }

    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

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
            Transform weapTransform = currWeapon.go.transform;
            Transform rayCheck = weapTransform.GetChild(weapTransform.childCount - 2);
            Ray shootingCheck = new Ray(rayCheck.position, rayCheck.transform.forward);
            RaycastHit _hit;
            Physics.Raycast(shootingCheck, out _hit, 1f);

            //Debug.DrawRay(shootingCheck.origin, shootingCheck.direction * 1f, Color.yellow);
            if(_hit.collider != null)
            {
                nextFire = Time.time + currWeapon.fireRate;
                currWeapon.currBullets--;
                return;
            }

            nextRecoil = Time.time + currWeapon.recoilRate;
            nextFire = Time.time + currWeapon.fireRate;
            Transform muzzle = weapTransform.GetChild(weapTransform.childCount - 1);
            muzzle.LookAt(target + aimOffset, Vector3.up);
            Ray ray = new Ray(muzzle.position, muzzle.transform.forward);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, range);
            //Debug.DrawRay(ray.origin, ray.direction * range, Color.cyan);
            //Debug.Log(hit.collider);
            if (hit.collider != null && !hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Weapon"))
            {
                GameObject go = Instantiate(bulletHole, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                Destroy(go, 10f);
                //Destroy(hit.collider.gameObject);
            }
            else if(hit.collider != null && hit.collider.CompareTag("Player"))
            {
                //Debug.LogError("My local viewID:" + photonView.ViewID);
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("DamagePlayer",
                    RpcTarget.Others,
                    hit.collider.gameObject.GetComponent<PhotonView>().ViewID,
                    currWeapon.damage,
                    photonView.ViewID
                    );
                
                PhotonNetwork.SendAllOutgoingCommands();
            }
            currWeapon.currBullets--;
        }
    }

    public void Attach(GameObject weapon, WeaponDB.Weapons refWeapon)
    {
        nextFire = 0;
        nextRecoil = 0;
        recoilTimer = 0;
        currWeapon.currRecoilStrength = 0;
        aimOffset = Vector3.zero;
        weapon.transform.SetParent(rightHand, false);
        weapon.transform.localPosition = refWeapon.handPos;
        weapon.transform.localEulerAngles = refWeapon.handRot;

        int pID = photonView.ViewID;

        photonView.RPC("SetWeaponHandParent", 
            RpcTarget.Others, 
            pID, 
            weapon.GetComponent<PhotonView>().ViewID,
            refWeapon.handPos,
            refWeapon.handRot,
            weapon.transform.localScale
            );
        PhotonNetwork.SendAllOutgoingCommands();
    }

    public void Detach(GameObject weapon, WeaponDB.Weapons refWeapon)
    {
        weapon.transform.SetParent(Spine, false);
        weapon.transform.localPosition = refWeapon.spinePos;
        weapon.transform.localEulerAngles = refWeapon.spineRot;

        int pID = photonView.ViewID;

        photonView.RPC("SetWeaponSpineParent",
            RpcTarget.Others,
            pID,
            weapon.GetComponent<PhotonView>().ViewID,
            refWeapon.spinePos,
            refWeapon.spineRot,
            weapon.transform.localScale
            );
        PhotonNetwork.SendAllOutgoingCommands();
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

            aimOffset = Vector3.Lerp(aimOffset, aimOffset + allRecoils, currWeapon.currRecoilStrength * Time.deltaTime);
            Vector3 diff = target - (target + aimOffset);
            //Debug.Log(diff);
            //player.camBase.transform.LookAt(target, Vector3.up);
            
            //player.cam.DOShakeRotation(0.5f, 0.5f, 2, 0);
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

    [PunRPC]
    public void DamagePlayer(int pID, int damage, int killerID)
    {
        PlayerController _player = PhotonView.Find(pID).gameObject.GetComponent<PlayerController>();
        if (!photonView.IsMine) return;
        if (_player.currHealth <= 0) return;

        _player.currHealth -= damage;
        if(_player.currHealth <= 0)
        {
            photonView.RPC("AddToFeed",
                RpcTarget.All,
                PhotonView.Find(killerID).Owner.NickName,
                PhotonView.Find(pID).Owner.NickName,
                killerID
                );
            //Debug.LogError("Killed by: " + PhotonView.Find(killerID).Owner.NickName);
        }
    }

    [PunRPC]
    public void AddToFeed(string p1, string p2, int killerID)
    {
        PlayerHUD.instance.addToFeed(p1, p2);

        PlayerController _player = PhotonView.Find(killerID).gameObject.GetComponent<PlayerController>();
        _player.kills++;
    }

    [PunRPC]
    public void SetWeaponHandParent(int pViewID, int wepViewID, Vector3 handPos, Vector3 handRot, Vector3 localScale)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.GetComponent<PhotonView>().ViewID == pViewID)
            {
                GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapon");

                foreach (GameObject weapon in weapons)
                {
                    if (weapon.GetComponent<PhotonView>().ViewID == wepViewID)
                    {
                        Transform hand = p.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand");
                        weapon.transform.SetParent(hand, false);
                        weapon.transform.localPosition = handPos;
                        weapon.transform.localEulerAngles = handRot;
                        weapon.transform.localScale = localScale;
                        break;
                    }
                }
                break;
            }
        }
    }

    [PunRPC]
    public void SetWeaponSpineParent(int pViewID, int wepViewID, Vector3 spinePos, Vector3 spineRot, Vector3 localScale)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.GetComponent<PhotonView>().ViewID == pViewID)
            {
                GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapon");

                foreach (GameObject weapon in weapons)
                {
                    if (weapon.GetComponent<PhotonView>().ViewID == wepViewID)
                    {
                        Transform spine = p.transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1");
                        weapon.transform.SetParent(spine, false);
                        weapon.transform.localPosition = spinePos;
                        weapon.transform.localEulerAngles = spineRot;
                        weapon.transform.localScale = localScale;
                        break;
                    }
                }
                break;
            }
        }
    }
}
