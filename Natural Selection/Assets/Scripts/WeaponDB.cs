using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponDB : MonoBehaviour
{
    public static WeaponDB instance;
    [System.Serializable]
    public class Recoil
    {
        public AnimationCurve curve;
        public Vector3 direction;
    }


    [System.Serializable]
    public class Weapons
    {
        public GameObject prefab;
        public GameObject go;
        public Vector3 handPos;
        public Vector3 handRot;
        public Vector3 spinePos;
        public Vector3 spineRot;
        public string name;
        public string type;
        public int bullets;
        public int reserve;
        public int currBullets;
        public int currReserve;
        public int damage;
        public int fireAmount;
        public float fireRate;
        public float animSpeed;
        public float reloadSpeed;
        public float recoilSpeed;
        public float recoilRate;
        public float LR_recoilStrength;
        public float MR_recoilStrength;
        public float HR_recoilStrength;
        public float currRecoilStrength;
        public Recoil[] recoils;
    }

    public Weapons[] gameWeapons;

    public Weapons GetWeaponByName(string name)
    {
        foreach(Weapons weapon in gameWeapons)
        {
            if (weapon.name == name)
            {
                return weapon;
            }
        }
        
        return null;
    }

    public List<Weapons> GetWeaponsByType(string type)
    {
        List<Weapons> ret = new List<Weapons>();
        foreach(Weapons weapon in gameWeapons)
        {
            if (weapon.type.Equals(type))
            {
                ret.Append(weapon);
            }
        }
        if (ret.Count > 0) return ret;

        return null;
    }

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        else
            instance = this;

    }
    private void Start()
    {
        //Debug.Log(instance.gameObject);
        DontDestroyOnLoad(this);
    }
}
