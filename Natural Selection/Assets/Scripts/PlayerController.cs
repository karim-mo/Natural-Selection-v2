using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static class States
    {
        public const int WEAPON_UP = 0;
        public const int WEAPON_DOWN = 1;
    }

    #region Publics
    [Header("Speed settings")]
    public float peakSpeed;
    public float mainSpeed;
    
    [Header("Gravity settings")]
    public float gravity;
    public float wallGravity;
    public float normalGravity;

    [Header("Jump settings")]
    public float jumpForce;
    public float jumpDrag;

    [Header("Dash settings")]
    public float groundDashForce;
    public float groundDashDuration;
    public float jumpDashForce;
    public float jumpDashDuration;

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
    private float m_x;
    private float m_z;
    private float m_prevX;
    private float mainCamFOV;
    private float aimFOV;
    private bool _grounded;
    private bool canMove;
    private bool rifleUp;
    private bool canShoot;
    private bool isReloading;
    private bool isGrabbingWep;
    private bool isHolsteringWep;
    private bool isCrouching;
    private bool m_xDecreased;
    private bool isGroundDashing;
    private bool isJumpDashing;
    private bool grabbingWall;

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

        m_x = Mathf.Clamp(Input.GetAxis("Horizontal") * 2, -1, 1);
        m_z = Mathf.Clamp(Input.GetAxis("Vertical") * 2, -1, 1);

        anim.SetFloat("VelX", m_x);
        anim.SetFloat("VelZ", m_z);

        //Debug.Log(m_z);
        //if (!isCrouching)
        //{
        //    anim.SetFloat("VelX", m_x);
        //    anim.SetFloat("VelZ", m_z);
        //}
        //else
        //{
        //    anim.SetFloat("VelX", _x);
        //    anim.SetFloat("VelZ", _z);
        //}

        _x = Input.GetAxisRaw("Horizontal");
        _z = Input.GetAxisRaw("Vertical");



        shootingHandling();
        handleRotation();

        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 0.25f, Color.cyan);

        if (grabbingWall)
        {
            Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 0.25f);
            print(hit.collider);
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                grabbingWall = true;
            }
            else
            {
                grabbingWall = false;
                gravity = normalGravity;
            }
        }

        _currSpeed = mainSpeed; // Debugging for now

        jump = Input.GetButtonDown("Jump");
        if (jump) Jump(Vector3.up);

        velAndAnimations();
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(transform.position, Vector3.forward * 20);
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


        //if (isDashing)
        //{
        //    _rb.position = Mathf.Lerp(_rb.position, transform.position + transform.TransformDirection(new Vector3(_x, 0, _z)), 10 * Time.fixedDeltaTime);
        //}

        //Debug.Log(_rb.velocity);
        // Gravity
        Vector3 grav = gravity * Vector3.up;
        _rb.AddForce(-grav * Time.fixedDeltaTime, ForceMode.Acceleration);
        //if (!grabbingWall)
        //{
        //    _rb.AddForce(-grav * Time.fixedDeltaTime, ForceMode.Acceleration);
        //}
        //else if(grabbingWall)
        //{
        //    _rb.AddForce(grav/4f * Time.fixedDeltaTime, ForceMode.Acceleration);
        //}
    }

    private void LateUpdate()
    {
        // Assuming marwan doesnt make the animation
        if (currState != States.WEAPON_UP) return;
        if (isGroundDashing || isJumpDashing) return;
        if (grabbingWall) return;

        Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        chest.LookAt(targetPos);
        if (!isCrouching)
            chest.Rotate(10, 45, 0, Space.Self);
        else
        {
            if (Mathf.Abs(_x) == 1 && Mathf.Abs(_z) == 0)
                chest.Rotate(20, 55, 0, Space.Self);
            else if (Mathf.Abs(_z) == 1 && Mathf.Abs(_x) == 0)
                chest.Rotate(0, 45, 0, Space.Self);
            else if (Mathf.Abs(_x) == 0 && Mathf.Abs(_z) == 0)
                chest.Rotate(20, 55, 0, Space.Self);
        }
    }
    #endregion

    public bool checkDecrease()
    {
        return m_x < m_prevX;
    }

    public bool isGrounded()
    {
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z), col.radius * .4f, jumpableLayers);
    }

    public void Move(Vector2 dir)
    {
        if (!canMove) return;
        if (isGroundDashing) return;

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
        if (!_grounded && !grabbingWall) return;
        if (!canMove) return;

        if (!grabbingWall)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.velocity += dir * jumpForce;
        }
        else
        {
            if (currState == States.WEAPON_UP)
            {
                anim.SetLayerWeight(1, 1);
            }
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.velocity += transform.TransformDirection(0, 1, -0.7f) * jumpForce;
        }
    }

    public void Dash()
    {
        if (_grounded)
            StartCoroutine(m_GDash());
        else
            StartCoroutine(m_JDash());
    }
    IEnumerator m_GDash()
    {
        Vector3 dir = transform.TransformDirection(new Vector3(_x, 0, _z));
        if (dir == Vector3.zero) yield break;
        isGroundDashing = true;
        transform.rotation = Quaternion.LookRotation(dir);
        if(currState == States.WEAPON_UP)
        {
            anim.SetLayerWeight(1, 0);
        }
        for (int i = 0; i < 50; i++)
        {
            if (!_grounded) continue;
            if (Input.GetMouseButton(0) && currState == States.WEAPON_UP) break;
            _rb.AddForce(transform.TransformDirection(Vector3.forward) * groundDashForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
            //_rb.velocity = cam.transform.TransformDirection(new Vector3(_x, 0, _z)) * groundDashForce;
            yield return new WaitForSeconds(groundDashDuration / 50);
        }
        if (currState == States.WEAPON_UP)
        {
            anim.SetLayerWeight(1, 1);
        }
        _rb.velocity = Vector3.zero;
        isGroundDashing = false;
    }

    IEnumerator m_JDash()
    {
        Vector3 dir = transform.TransformDirection(new Vector3(_x, 0, _z));
        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        if (dir == Vector3.zero) yield break;
        isJumpDashing = true;
        transform.rotation = Quaternion.LookRotation(dir);
        gravity = 0;
        if (currState == States.WEAPON_UP)
        {
            anim.SetLayerWeight(1, 0);
        }
        for (int i = 0; i < 50; i++)
        {
            if (_grounded) break;
            _rb.AddForce(transform.TransformDirection(Vector3.forward) * jumpDashForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
            yield return new WaitForSeconds(jumpDashDuration / 50);
        }
        if (currState == States.WEAPON_UP)
        {
            anim.SetLayerWeight(1, 1);
        }
        gravity = normalGravity;
        _rb.velocity = Vector3.zero;
        isJumpDashing = false;
    }


    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Wall" && !_grounded && _rb.velocity.y < 0)
        {
            if (currState == States.WEAPON_UP)
            {
                anim.SetLayerWeight(1, 0);
            }
            grabbingWall = true;
            gravity = wallGravity;
            transform.rotation = Quaternion.LookRotation(-collision.GetContact(0).normal, Vector3.up);
        }
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
        anim.SetBool("isGroundDashing", isGroundDashing);
        anim.SetBool("isJumpDashing", isJumpDashing);
        anim.SetBool("WallSlide", grabbingWall);

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
        isCrouching = false;
        grabbingWall = false;
        gravity = normalGravity;
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
        if (isGroundDashing) return;
        if (isJumpDashing) return;
        if (grabbingWall) return;

        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, camBase.gameObject.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1);
    }

    public void statesHanlder()
    {
        currState = rifleUp ? States.WEAPON_UP : States.WEAPON_DOWN;
        _grounded = isGrounded();

        if (_grounded && !isGroundDashing)
        {
            if (currState == States.WEAPON_UP)
            {
                anim.SetLayerWeight(1, 1);
            }
            grabbingWall = false;
            gravity = normalGravity;
        }
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

        //if (Input.GetKey(KeyCode.C) && currState == States.WEAPON_UP)
        //{
        //    isCrouching = true;
        //    anim.SetLayerWeight(2, 1);
        //}
        //else
        //{
        //    isCrouching = false;
        //    //anim.SetLayerWeight(2, 0);
        //}
        //if (Input.GetKeyDown(KeyCode.LeftControl) && currState == States.WEAPON_UP)
        //{
        //    isCrouching = !isCrouching;
        //    if (isCrouching) anim.SetLayerWeight(2, 1);
        //    else anim.SetLayerWeight(2, 0);

        //}
        //if(!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D) && isCrouching)
        //{
        //    anim.SetFloat("VelX", 0);
        //    anim.SetFloat("VelZ", 0);
        //}

        if (Input.GetKeyDown(KeyCode.F))
        {
            Dash();
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
