using Photon.Pun;
using System.Collections;
using System.Xml.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public static class States
    {
        public const int WEAPON_UP = 0;
        public const int WEAPON_DOWN = 1;
    }

    #region Publics
    [Header("Player stats")]
    public float maxHealth;
    public float currHealth;
    

    [Header("Speed settings")]
    public float aimSpeed;
    public float runningSpeed;
    public float strafeSpeed;
    public float backwardSpeed;
    public float diagonalRate;
    public float runningRate;
    public float backwardRate;
    public float strafeRate;
    public float aimRate;
    public float rifleRunningRate;
    public float rifleStrafeRate;
    public float rifleBackwardRate;
    public float rifleDiagonalRate;


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
    [HideInInspector]
    public bool fire;
    [HideInInspector]
    public Vector3 aimOffset;
    [HideInInspector]
    public bool isReloading;
    [HideInInspector]
    public Camera cam;
    [HideInInspector]
    public Vector3 targetPos;
    [HideInInspector]
    public bool m_isGrabbingWep;
    [HideInInspector]
    public bool isHolsteringWep;
    [HideInInspector]
    public bool H_isRifleUp;
    [HideInInspector]
    public bool G_isRifleUp;
    [HideInInspector]
    public bool R_reloaded;
    [HideInInspector]
    public bool D_isDead;
    [HideInInspector]
    public ShootingHandling weapon;
    [HideInInspector]
    public int kills;
    [HideInInspector]
    public int deaths;
    [HideInInspector]
    public bool canMove;
    [HideInInspector]
    public bool isGroundDashing;
    [HideInInspector]
    public bool isJumpDashing;
    [HideInInspector]
    public AudioManager _audio;

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
    private float footRate;
    private float nextFootstep;
    private bool _grounded;
    private bool rifleUp;
    private bool canShoot;
    private bool isGrabbingWep;
    private bool isCrouching;
    private bool m_xDecreased;
    private bool grabbingWall;
    private bool Aim;
    private bool Dead;

    private Rigidbody _rb;
    private Animator anim;
    private CapsuleCollider col;
    private NetworkController networkedPlayer;
    private StaminaHandling stamina;
    #endregion

    void Start()
    {       
        varsInit();
        refInit();
        //currentSnapshot = 
    }

    #region Updates
    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;
        if (!canMove) return;

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
        /*
        // Should work without detecting inputs since the axis are raw but that's just an extra step
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && !Aim) _currSpeed = runningSpeed;
        else if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && Mathf.Abs(_x) >= 0 && !Aim) _currSpeed = backwardSpeed;
        else if (((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) || (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))) && !Aim && _z == 0) _currSpeed = strafeSpeed;
        else if (Aim) _currSpeed = aimSpeed;
        */
        //Fuck the above solution in particular, feels hardcoded
        if (_z > 0 && Mathf.Abs(_x) >= 0 && !Aim)
        {
            _currSpeed = runningSpeed;
            footRate = runningRate;
        }
        else if (_z < 0 && Mathf.Abs(_x) >= 0 && !Aim)
        {
            _currSpeed = backwardSpeed;
            footRate = backwardRate;
        }
        else if (Mathf.Abs(_x) > 0 && !Aim && _z == 0)
        {
            _currSpeed = strafeSpeed;
            footRate = strafeRate;
        }
        else if (Aim)
        {
            _currSpeed = aimSpeed;
            footRate = aimRate;
        }



        if (_z > 0 && Mathf.Abs(_x) > 0 && !Aim) footRate = diagonalRate;


        if (_z > 0 && Mathf.Abs(_x) >= 0 && !Aim && currState == States.WEAPON_UP)
        {
            footRate = rifleRunningRate;
        }
        else if (_z < 0 && Mathf.Abs(_x) >= 0 && !Aim && currState == States.WEAPON_UP)
        {
            footRate = rifleBackwardRate;
        }
        else if (Mathf.Abs(_x) > 0 && !Aim && _z == 0 && currState == States.WEAPON_UP)
        {
            footRate = rifleStrafeRate;
        }

        if (_z > 0 && Mathf.Abs(_x) > 0 && !Aim && currState == States.WEAPON_UP) footRate = rifleDiagonalRate;

        //if (currState == States.WEAPON_UP) footRate = rifleRate;

        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 0.25f, Color.cyan);

        if (grabbingWall)
        {
            Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 0.25f);
            //print(hit.collider.tag);
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

        //_currSpeed = runningSpeed; // Debugging for now

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
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;


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


        // Gravity
        Vector3 grav = gravity * Vector3.up;
        _rb.AddForce(-grav * Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            if (currState != States.WEAPON_UP) return;

            Transform _chest = anim.GetBoneTransform(HumanBodyBones.Chest);
            //_chest.rotation = Quaternion.RotateTowards(_chest.rotation, Quaternion.LookRotation((targetPos - transform.position).normalized), 180 * Time.deltaTime);
            _chest.LookAt(targetPos);
            _chest.Rotate(10, 45, 0, Space.Self);
            return;
        }

        // Assuming marwan doesnt make the animation
        if (currState != States.WEAPON_UP) return;
        if (isGroundDashing || isJumpDashing) return;
        if (grabbingWall) return;
        if (!canMove) return;

        Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        chest.LookAt(targetPos + weapon.aimOffset);
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

        //Debug.Log(dir.magnitude);

        if(dir.magnitude > 0.1f && Time.time > nextFootstep && !isGroundDashing && !isJumpDashing && _grounded)
        {
            //Debug.Log("haha");
            nextFootstep = Time.time + footRate;
            AudioManager.Sound randomClip = _audio.sounds[Random.Range(0, 4)];
            _audio.PlayOne(randomClip.name);
            photonView.RPC("playAudio", 
                RpcTarget.Others, 
                randomClip.name,
                0,
                false
                );
            //GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
        }
        //else if((dir.magnitude < 0.05f && footRate != strafeRate && footRate != rifleStrafeRate) || isGroundDashing || isJumpDashing || !_grounded)
        //{
        //    //audio.Stop("Running");
        //    for (int i = 0; i < 4; i++)
        //    {
        //        AudioManager.Sound randomClip = _audio.sounds[i];
        //        _audio.Stop(randomClip.name);
        //    }
        //}

        if (_grounded)
            _rb.velocity = transform.TransformDirection(dir.x, _rb.velocity.y, dir.y);
        else
        {
            Vector3 F = transform.TransformDirection(dir.x * jumpDrag, 0, dir.y * jumpDrag) * Time.fixedDeltaTime;
            Vector3 V = (F / _rb.mass) * Time.fixedDeltaTime + _rb.velocity;

            if (velocityCalc(V) < runningSpeed)
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
        if (currState != States.WEAPON_UP) return;
        if (grabbingWall) return;
        if (isGroundDashing || isJumpDashing) return;
        if (isGrabbingWep || isHolsteringWep) return;
        if (isReloading) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //Debug.DrawRay(ray.origin, ray.direction * 200f, Color.red);
        RaycastHit hit;
        fire = Input.GetButton("Fire1");
        if (Physics.Raycast(ray, out hit, 200f))
        {
            targetPos = hit.point;
            
            if (fire)
            {
                if (hit.distance <= 8f)
                {
                    weapon.currWeapon.currRecoilStrength = weapon.currWeapon.LR_recoilStrength;
                }
                else if (hit.distance <= 20f)
                {
                    weapon.currWeapon.currRecoilStrength = weapon.currWeapon.MR_recoilStrength;
                }
                else if (hit.distance > 20f)
                {
                    weapon.currWeapon.currRecoilStrength = weapon.currWeapon.HR_recoilStrength;
                }


                weapon.Shoot(targetPos, hit.distance);
            }
        }
        else
        {
            targetPos = ray.GetPoint(200f);
            weapon.Shoot(targetPos, 200f);
        }
    }

    public float velocityCalc(Vector3 velocity)
    {
        return Mathf.Sqrt((velocity.x * velocity.x) + (velocity.z * velocity.z)); 
    }

    public void velAndAnimations()
    {
        if (Dead) return;
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
        Aim = false;
        H_isRifleUp = true;
        G_isRifleUp = false;
        R_reloaded = false;
        Dead = false;
        D_isDead = false;
        currHealth = maxHealth;
        gravity = normalGravity;
    }

    public void refInit()
    {
        _rb = GetComponent<Rigidbody>();
        stamina = GetComponent<StaminaHandling>();
        anim = GetComponent<Animator>();
        camBase = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        cam = camBase.gameObject.transform.GetChild(0).gameObject.GetComponent<Camera>();
        col = GetComponent<CapsuleCollider>();
        weapon = GetComponent<ShootingHandling>();
        networkedPlayer = GetComponent<NetworkController>();
        _audio = GetComponent<AudioManager>();
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

        if(currHealth <= 0 && !Dead)
        {
            Dead = true;
            canMove = false;
            deaths++;
            StartCoroutine("Death");
            return;
        }

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

        if (Input.GetKeyDown(KeyCode.R))
        {
            m_Reload();
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

        if (Input.GetKeyDown(KeyCode.Alpha1) && currState == States.WEAPON_DOWN && !isReloading && !isHolsteringWep && !isGrabbingWep)
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
            if (weapon.currWeapon.go == null)
            {
                if (!PhotonNetwork.OfflineMode)
                {
                    weapon.currWeapon.go =
                        PhotonNetwork.Instantiate(weapon.currWeapon.prefab.name,
                        weapon.currWeapon.prefab.transform.position,
                        weapon.currWeapon.prefab.transform.rotation
                        );
                }
                else
                {
                    weapon.currWeapon.go =
                        Instantiate(weapon.currWeapon.prefab,
                        weapon.currWeapon.prefab.transform.position,
                        weapon.currWeapon.prefab.transform.rotation
                        );
                }
            }
            StartCoroutine("GrabWeapon");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && currState == States.WEAPON_DOWN && !isReloading && !isHolsteringWep && !isGrabbingWep)
        {
            weapon.currWeapon = weapon.currWeapons[1];
            if (weapon.currWeapon.go == null)
            {
                if (!PhotonNetwork.OfflineMode)
                {
                    weapon.currWeapon.go =
                        PhotonNetwork.Instantiate(weapon.currWeapon.prefab.name,
                        weapon.currWeapon.prefab.transform.position,
                        weapon.currWeapon.prefab.transform.rotation
                        );
                }
                else
                {
                    weapon.currWeapon.go =
                        Instantiate(weapon.currWeapon.prefab,
                        weapon.currWeapon.prefab.transform.position,
                        weapon.currWeapon.prefab.transform.rotation
                        );
                }
            }
            StartCoroutine("GrabWeapon");
        }

        if (Input.GetMouseButton(1) && currState == States.WEAPON_UP && !isReloading && !isHolsteringWep && !isGrabbingWep)
        {
            Aim = true;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, aimFOV, Time.deltaTime * 15);
        }
        else
        {
            Aim = false;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, mainCamFOV, Time.deltaTime * 10);
        }

        if (Input.GetKeyDown(KeyCode.F) && !isReloading && !isHolsteringWep && !isGrabbingWep && !isGroundDashing && !isJumpDashing && !grabbingWall && stamina.canUseStamina())
        {
            Dash();
            stamina.m_currStamina -= 30;
            stamina.stopRecharge();
        }
    }

    public void m_Reload()
    {
        if (currState != States.WEAPON_UP) return;
        if (isReloading || isGrabbingWep || isHolsteringWep) return;
        if (weapon.currWeapon.currBullets >= weapon.currWeapon.bullets) return;
        if (weapon.currWeapon.currReserve <= 0) return;

        StartCoroutine("Reload");
    }
    
    public void playSound(string clip)
    {
        _audio.PlayOne(clip);
    }

    IEnumerator HolsterWeapon()
    {
        canShoot = false;
        isHolsteringWep = true;
        anim.SetTrigger("Holster");
        photonView.RPC("sendTrigger", 
            RpcTarget.Others, 
            "Holster", 
            photonView.ViewID
            );
        PhotonNetwork.SendAllOutgoingCommands();
        anim.SetInteger("aimState", 0);
        while (H_isRifleUp)
        {
            yield return null;
        }
        weapon.currWeapon = weapon.currWeapons[1];
        PhotonNetwork.Destroy(weapon.currWeapon.go);
        weapon.currWeapon = weapon.currWeapons[0];
        weapon.Detach(weapon.currWeapon.go, weapon.currWeapon);
        weapon.currWeapon = null;
        rifleUp = false;
        H_isRifleUp = true;
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
            isHolsteringWep = true;
            anim.SetTrigger("Holster");
            photonView.RPC("sendTrigger", 
                RpcTarget.Others, 
                "Holster", 
                photonView.ViewID
                );
            PhotonNetwork.SendAllOutgoingCommands();
            anim.SetInteger("aimState", 0);
            while (H_isRifleUp)
            {
                yield return null;
            }
            isHolsteringWep = false;
            H_isRifleUp = true;
            weapon.Detach(weapon.prevWeapon.go, weapon.prevWeapon);
            weapon.Attach(weapon.currWeapon.go, weapon.currWeapon);
        }
        m_isGrabbingWep = true;
        anim.SetTrigger("GrabWeapon");
        photonView.RPC("sendTrigger", 
            RpcTarget.Others, 
            "GrabWeapon", 
            photonView.ViewID
            );
        PhotonNetwork.SendAllOutgoingCommands();
        while (!G_isRifleUp)
        {
            yield return null;
        }
        m_isGrabbingWep = false;
        rifleUp = true;
        G_isRifleUp = false;
        anim.SetInteger("aimState", 1);
        canShoot = true;
        isGrabbingWep = false;
        _audio.Play("WeaponOut");
    }

    IEnumerator Reload()
    {
        isReloading = true;
        canShoot = false;
        anim.SetTrigger("Reload");
        photonView.RPC("sendTrigger", 
            RpcTarget.Others, 
            "Reload", 
            photonView.ViewID
            );
        PhotonNetwork.SendAllOutgoingCommands();
        _audio.Play("Reload");
        while (!R_reloaded)
        {
            yield return null;
        }
        canShoot = true;
        R_reloaded = false;
        weapon.Reload();
        isReloading = false;
    }

    IEnumerator Death()
    {
        anim.SetBool("isDead", Dead);
        if (weapon.currWeapon != null)
        {
            weapon.currWeapon = weapon.currWeapons[1];
            if(weapon.currWeapon.go != null) PhotonNetwork.Destroy(weapon.currWeapon.go);
            weapon.currWeapon = weapon.currWeapons[0];
            weapon.Detach(weapon.currWeapon.go, weapon.currWeapon);
            weapon.currWeapon = null;
        }
        rifleUp = false;
        isHolsteringWep = false;
        isGrabbingWep = false;
        isReloading = false;
        anim.SetLayerWeight(1, 0);
        //canMove = false;
        anim.SetInteger("state", 3);
        yield return new WaitForSeconds(7f);
        currHealth = maxHealth;
        weapon.currWeapons[0].currBullets = weapon.currWeapons[0].bullets;
        weapon.currWeapons[0].currReserve = weapon.currWeapons[0].reserve;
        weapon.currWeapons[1].currBullets = weapon.currWeapons[1].bullets;
        weapon.currWeapons[1].currReserve = weapon.currWeapons[1].reserve;
        Dead = false;
        anim.SetBool("isDead", Dead);
        anim.SetInteger("aimState", 0);
        anim.SetInteger("state", 0);
        GameObject[] spawns = FindObjectOfType<NetworkMgr>().spawnPoints;
        transform.position = spawns[Random.Range(0, 1000 + 1) % spawns.Length].transform.position;
        canMove = true;
    }

    [PunRPC]
    public void sendTrigger(string trigger, int pID)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players)
        {
            if(player.GetComponent<PhotonView>().ViewID == pID)
            {
                player.GetComponent<Animator>().SetTrigger(trigger);
                return;
            }
        }
    }

    [PunRPC]
    public void playAudio(string name, int wepID, bool shoot)
    {
        AudioSource.PlayClipAtPoint(_audio.Find(name), transform.position);
        if (shoot)
        {
            Transform weapon = PhotonView.Find(wepID).gameObject.transform;
            Transform muzzle = weapon.GetChild(weapon.childCount - 1);
            ParticleSystem muzzleFlash = muzzle.GetChild(0).GetComponent<ParticleSystem>();
            muzzleFlash.Play();
        }
    }
    //[PunRPC]
    //void updateVelocity(Vector3 v, Vector3 p)
    //{
    //    if (photonView.IsMine) return;

    //    currentSnapshot = new Snapshot(transform.position, _rb.velocity, p, v, 1 / 20f);
    //}

    public bool isMine()
    {
        return photonView.IsMine;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(targetPos);
            stream.SendNext(currState);
            stream.SendNext(currHealth);
            stream.SendNext(kills);
            stream.SendNext(deaths);

            //stream.SendNext(isGroundDashing);
            //stream.SendNext(isJumpDashing);
            //stream.SendNext(_grounded);
            //stream.SendNext(footRate);
            //stream.SendNext(m_isGrabbingWep);
            //stream.SendNext(isHolsteringWep);
            //stream.SendNext(isReloading);
            
            //stream.SendNext(_rb.velocity);

            //stream.SendNext(_currSpeed);
            //stream.SendNext(gravity);
            //stream.SendNext(grabbingWall);
            //stream.SendNext(isJumpDashing);
        }
        else
        {
            networkedPlayer.m_networkedPosition = (Vector3)stream.ReceiveNext();
            networkedPlayer.m_networkedRotation = (Quaternion)stream.ReceiveNext();
            targetPos = (Vector3)stream.ReceiveNext();
            currState = (int)stream.ReceiveNext();
            currHealth = (float)stream.ReceiveNext();
            kills = (int)stream.ReceiveNext();
            deaths = (int)stream.ReceiveNext();

            //networkedPlayer.isGroundDashing = (bool)stream.ReceiveNext();
            //networkedPlayer.isJumpDashing = (bool)stream.ReceiveNext();
            //networkedPlayer._grounded = (bool)stream.ReceiveNext();
            //networkedPlayer.footRate = (float)stream.ReceiveNext();

            //m_isGrabbingWep = (bool)stream.ReceiveNext();
            //isHolsteringWep = (bool)stream.ReceiveNext();
            //isReloading = (bool)stream.ReceiveNext();
            //m_networkedVelocity = (Vector3)stream.ReceiveNext();

            //photonView.RPC("updateVelocity", RpcTarget.All, m_networkedPosition, _rb.velocity);
            //PhotonNetwork.SendAllOutgoingCommands();

            //_currSpeed = (float)stream.ReceiveNext();
            //gravity = (float)stream.ReceiveNext();
            //grabbingWall = (bool)stream.ReceiveNext();
            //isJumpDashing = (bool)stream.ReceiveNext();


            //currTimer = 0;
            //lastTimePacketSent = m_timePacketSent;
            //m_timePacketSent = (float)info.SentServerTime;
            //lastNetworkedPosition = transform.position;
        }

    }
}
