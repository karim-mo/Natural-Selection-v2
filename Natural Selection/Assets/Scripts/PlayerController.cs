using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static class States
    {
        public const int WEAPON_UP = 0;
        public const int WEAPON_DOWN = 1;
    }

    #region Publics
    [Header("Physics stats")]
    public float peakSpeed;
    public float mainSpeed;
    public float jumpForce;
    public float gravity;
    public float jumpDrag;

    [Header("Components&References")]
    public CameraController camBase;

    [Header("Adjustables")]
    public LayerMask jumpableLayers;

    [Header("Not So Public")]
    [HideInInspector]
    public bool jump;
    [HideInInspector]
    public int currState = -1;
    #endregion


    #region Privates
    [Header("Privates")]
    private float _currSpeed;
    private float _x;
    private float _z;
    private float mainCamFOV;
    private float aimFOV;
    private bool _grounded;
    private bool canMove;
    private bool rifleUp;
    private bool canShoot;
    private bool isReloading;
    private bool isGrabbingWep;
    private bool isHolsteringWep;

    private Rigidbody _rb;
    private Animator anim;
    private CapsuleCollider col;
    private Camera cam;
    private ShootingHandling weapon;

    private RaycastHit _ground;
    private Vector3 _groundLoc;
    private Vector3 targetPos;
    #endregion

    void Start()
    {
        varsInit();
        refInit();
    }

    #region Updates
    void Update()
    {
        statesHanlder();

        _x = Mathf.Clamp(Input.GetAxis("Horizontal") * 2, -1, 1);
        _z = Mathf.Clamp(Input.GetAxis("Vertical") * 2, -1, 1);

        anim.SetFloat("VelX", _x);
        anim.SetFloat("VelZ", _z);

        _x = Input.GetAxisRaw("Horizontal");
        _z = Input.GetAxisRaw("Vertical");



        shootingHandling();
        handleRotation();
        

        _currSpeed = mainSpeed; // Debugging for now

        jump = Input.GetButtonDown("Jump");
        if (jump) Jump(Vector3.up);

        velAndAnimations();
    }

    
    void FixedUpdate()
    {
        float finalSpeedX = _currSpeed * _x;
        float finalSpeedZ = _currSpeed * _z;

        if(Mathf.Abs(_x) > 0 && Mathf.Abs(_z) > 0)
        {
            finalSpeedX /= 1.414f;
            finalSpeedZ /= 1.414f;
        }

        Move(new Vector2(finalSpeedX, finalSpeedZ));
        



        // Gravity
        Vector3 grav = -gravity * Vector3.up;
        _rb.AddForce(grav * Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    private void LateUpdate()
    {
        if (currState != States.WEAPON_UP) return;

        Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        chest.LookAt(targetPos);
        chest.Rotate(10, 45, 0, Space.Self);
    }
    #endregion

    public bool isGrounded()
    {
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z), col.radius * .4f, jumpableLayers);
    }

    public void Move(Vector2 dir)
    {
        if (!canMove) return;

        if (_grounded)
            _rb.velocity = transform.TransformDirection(dir.x, _rb.velocity.y, dir.y);
        else
        {
            Vector3 F = transform.TransformDirection(dir.x * jumpDrag, 0, dir.y * jumpDrag) * Time.fixedDeltaTime;
            Vector3 V = (F / _rb.mass) * Time.fixedDeltaTime + _rb.velocity;

            if (velocityCalc(V) < mainSpeed)
            {
                _rb.AddForce(transform.TransformDirection(dir.x * jumpDrag, 0, dir.y * jumpDrag) * Time.fixedDeltaTime);
            }

            _rb.AddForce(new Vector3(-_rb.velocity.x * jumpDrag, 0, -_rb.velocity.z * jumpDrag) * Time.fixedDeltaTime);
        } 
    }

    public void Jump(Vector3 dir)
    {
        if (!_grounded) return;
        if (!canMove) return;
        
        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        _rb.velocity += dir * jumpForce;
    }

    public void shootingHandling()
    {
        if (!canMove) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(ray.origin, ray.direction * 40f, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 40f))
        {
            //Debug.Log(hit.point + " " + hit.collider.gameObject.name);
            targetPos = hit.point;
        }
        else
        {
            //Debug.Log(ray.GetPoint(20f));
            targetPos = ray.GetPoint(20f);
        }


    }

    public float velocityCalc(Vector3 velocity)
    {
        return Mathf.Sqrt((velocity.x * velocity.x) + (velocity.z * velocity.z)); 
    }

    public void velAndAnimations()
    {
        anim.SetBool("Grounded", _grounded);
        anim.SetBool("Rifle", rifleUp);

        if (_rb.velocity.y > (0 + 1.5f))
        {
            anim.SetInteger("state", 2);
        }
        else if (_rb.velocity.y < (0 - 1.5f) && !_grounded)
        {
            anim.SetInteger("state", 1);
        }
        else anim.SetInteger("state", 0);
    }

    public void varsInit()
    {
        currState = -1;
        aimFOV = 40;
        mainCamFOV = 60;
        _grounded = false;
        canMove = true;
        rifleUp = false;
        canShoot = false;
        isReloading = false;
        isGrabbingWep = false;
        isHolsteringWep = false;
    }

    public void refInit()
    {
        _rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        camBase = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        cam = camBase.gameObject.transform.GetChild(0).gameObject.GetComponent<Camera>();
        col = GetComponent<CapsuleCollider>();
        weapon = GetComponent<ShootingHandling>();
    }

    public void handleRotation()
    {
        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, camBase.gameObject.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        transform.rotation = rot;
    }

    public void statesHanlder()
    {
        currState = rifleUp ? States.WEAPON_UP : States.WEAPON_DOWN;
        _grounded = isGrounded();

        if (Input.GetKeyDown(KeyCode.Q) && _grounded && !isReloading && !isGrabbingWep && !isHolsteringWep && currState == States.WEAPON_UP)
        {
            StartCoroutine("HolsterWeapon");
        }

        if (Input.GetKeyDown(KeyCode.R) && currState == States.WEAPON_UP && !isReloading && !isGrabbingWep && !isHolsteringWep)
        {
            StartCoroutine("Reload");
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && currState == States.WEAPON_UP && !isReloading && !isHolsteringWep && !isGrabbingWep)
        {
            weapon.prevWeapon = weapon.currWeapon;
            weapon.currWeapon = weapon.currWeapons[0];
            if (weapon.prevWeapon == weapon.currWeapon)
            {
                weapon.prevWeapon = weapon.currWeapons[1];
                return;
            }
            StartCoroutine("GrabWeapon");
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && currState == States.WEAPON_DOWN)
        {
            weapon.currWeapon = weapon.currWeapons[0];
            StartCoroutine("GrabWeapon");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && currState == States.WEAPON_UP && !isReloading && !isHolsteringWep && !isGrabbingWep)
        {
            weapon.prevWeapon = weapon.currWeapon;
            weapon.currWeapon = weapon.currWeapons[1];
            if(weapon.prevWeapon == weapon.currWeapon)
            {
                weapon.prevWeapon = weapon.currWeapons[0];
                return;
            }
            if (weapon.currWeapon.go == null) weapon.currWeapon.go = Instantiate(weapon.currWeapon.prefab);
            StartCoroutine("GrabWeapon");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && currState == States.WEAPON_DOWN)
        {
            weapon.currWeapon = weapon.currWeapons[1];
            if(weapon.currWeapon.go == null) weapon.currWeapon.go = Instantiate(weapon.currWeapon.prefab);
            StartCoroutine("GrabWeapon");
        }

        if (Input.GetMouseButton(1) && currState == States.WEAPON_UP && !isReloading && !isHolsteringWep && !isGrabbingWep)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, aimFOV, Time.deltaTime * 15);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, mainCamFOV, Time.deltaTime * 10);
        }
    }

    IEnumerator HolsterWeapon()
    {
        canShoot = false;
        isHolsteringWep = true;
        anim.SetTrigger("Holster");
        anim.SetInteger("aimState", 0);
        while (HolsterBehaviour.isRifleUp)
        {
            yield return null;
        }
        weapon.currWeapon = weapon.currWeapons[1];
        Destroy(weapon.currWeapon.go);
        weapon.currWeapon = weapon.currWeapons[0];
        weapon.Detach(weapon.currWeapon.go, weapon.currWeapon);
        weapon.currWeapon = null;
        rifleUp = false;
        HolsterBehaviour.isRifleUp = true;
        anim.SetLayerWeight(1, 0);
        isHolsteringWep = false;
    }

    IEnumerator GrabWeapon()
    {
        isGrabbingWep = true;
        anim.SetLayerWeight(1, 1);
        if(currState == States.WEAPON_DOWN)
        {
            weapon.Attach(weapon.currWeapon.go, weapon.currWeapon);
        }
        else
        {
            anim.SetTrigger("Holster");
            anim.SetInteger("aimState", 0);
            while (HolsterBehaviour.isRifleUp)
            {
                yield return null;
            }
            HolsterBehaviour.isRifleUp = true;
            weapon.Detach(weapon.prevWeapon.go, weapon.prevWeapon);
            weapon.Attach(weapon.currWeapon.go, weapon.currWeapon);
        }
        anim.SetTrigger("GrabWeapon");
        while (!GrabWeaponBehaviour.isRifleUp)
        {
            yield return null;
        }
        rifleUp = true;
        GrabWeaponBehaviour.isRifleUp = false;
        anim.SetInteger("aimState", 1);
        canShoot = true;
        isGrabbingWep = false;
    }

    IEnumerator Reload()
    {
        isReloading = true;
        canShoot = false;
        anim.SetTrigger("Reload");
        while (!Reloading.Reloaded)
        {
            yield return null;
        }
        canShoot = true;
        Reloading.Reloaded = false;
        isReloading = false;
    }
}
