using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    struct Snapshot
    {
        readonly Vector3 positionOnArrival;
        readonly Vector3 velocityOnArrival;
        readonly Vector3 reportedPosition;
        readonly Vector3 reportedVelocity;
        // Using floats for time works well for games that can go for hours.
        // Switch to doubles or other formats for games expected to run persistently.
        readonly float arrivalTime;
        readonly float blendWindow;

        public Snapshot(Vector3 currentPosition, Vector3 currentVelocity,
                       Vector3 reportedPosition, Vector3 reportedVelocity,
                       float blendWindow)
        {
            arrivalTime = Time.time;
            positionOnArrival = currentPosition;
            velocityOnArrival = currentVelocity;
            this.reportedPosition = reportedPosition;
            this.reportedVelocity = reportedVelocity;
            this.blendWindow = blendWindow;
        }

        public Vector3 EstimatePosition(float time, out Vector3 velocity)
        {
            float dT = time - arrivalTime;
            float blend = Mathf.Clamp01(dT / blendWindow);

            velocity = Vector3.Lerp(velocityOnArrival, reportedVelocity, blend);

            Vector3 position = Vector3.Lerp(
                       positionOnArrival + velocity * dT,
                       reportedPosition + reportedVelocity * dT,
                       blend);

            return position;
        }
    }

    public static class States
    {
        public const int WEAPON_UP = 0;
        public const int WEAPON_DOWN = 1;
    }

    #region Publics
    [Header("Speed settings")]
    public float aimSpeed;
    public float runningSpeed;
    public float strafeSpeed;
    public float backwardSpeed;
    
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
    private bool isGrabbingWep;
    private bool m_isGrabbingWep;
    private bool isHolsteringWep;
    private bool isCrouching;
    private bool m_xDecreased;
    private bool isGroundDashing;
    private bool isJumpDashing;
    private bool grabbingWall;
    private bool Aim;

    private Rigidbody _rb;
    private Animator anim;
    private CapsuleCollider col;
    private ShootingHandling weapon;
    private StaminaHandling stamina;

    private RaycastHit _ground;
    private Vector3 _groundLoc;
    private Vector3 m_networkedPosition;
    private Vector3 lastNetworkedPosition;
    private Quaternion m_networkedRotation;
    Snapshot currentSnapshot;

    private float m_timePacketSent;
    private float lastTimePacketSent;
    private float currTimer = 0;
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
        photonView.RPC("updateVelocity", RpcTarget.All, m_networkedPosition, _rb.velocity);
        PhotonNetwork.SendAllOutgoingCommands();
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            //transform.position = Vector3.Slerp(transform.position, m_networkedPosition, 15 * Time.deltaTime);
            transform.position = currentSnapshot.EstimatePosition(Time.time, out Vector3 velocity);
            _rb.velocity = velocity;
            //Debug.Log(_rb.velocity);
            //updateNetworkPosition();
            //updateNetworkPosition();
            //Debug.Log(GrabWeaponBehaviour.isRifleUp);
            updateNetworkAnims();
            updateNetworkRotation();
            return;
        }
    
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
        if (_z > 0 && Mathf.Abs(_x) >= 0 && !Aim) _currSpeed = runningSpeed;
        else if (_z < 0 && Mathf.Abs(_x) >= 0 && !Aim) _currSpeed = backwardSpeed;
        else if (Mathf.Abs(_x) > 0 && !Aim && _z == 0) _currSpeed = strafeSpeed;
        else if (Aim) _currSpeed = aimSpeed;

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
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            
            return;
        }

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
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        // Assuming marwan doesnt make the animation
        if (currState != States.WEAPON_UP) return;
        if (isGroundDashing || isJumpDashing) return;
        if (grabbingWall) return;

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
        Debug.DrawRay(ray.origin, ray.direction * 200f, Color.red);
        RaycastHit hit;
        fire = Input.GetButton("Fire1");
        if (Physics.Raycast(ray, out hit, 200f))
        {
            targetPos = hit.point;
            
            if (fire)
            {
                weapon.Shoot(targetPos, 200f);
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
            if (weapon.currWeapon.go == null) weapon.currWeapon.go = Instantiate(weapon.currWeapon.prefab);
            StartCoroutine("GrabWeapon");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && currState == States.WEAPON_DOWN && !isReloading && !isHolsteringWep && !isGrabbingWep)
        {
            weapon.currWeapon = weapon.currWeapons[1];
            if(weapon.currWeapon.go == null) weapon.currWeapon.go = Instantiate(weapon.currWeapon.prefab);
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

        if (Input.GetKeyDown(KeyCode.F) && !isReloading && !isHolsteringWep && !isGrabbingWep && !isGroundDashing && !isJumpDashing && !grabbingWall)
        {
            Dash();
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
            isHolsteringWep = true;
            anim.SetTrigger("Holster");
            anim.SetInteger("aimState", 0);
            while (HolsterBehaviour.isRifleUp)
            {
                yield return null;
            }
            isHolsteringWep = false;
            HolsterBehaviour.isRifleUp = true;
            weapon.Detach(weapon.prevWeapon.go, weapon.prevWeapon);
            weapon.Attach(weapon.currWeapon.go, weapon.currWeapon);
        }
        m_isGrabbingWep = true;
        anim.SetTrigger("GrabWeapon");
        while (!GrabWeaponBehaviour.isRifleUp)
        {
            yield return null;
        }
        m_isGrabbingWep = false;
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
        weapon.Reload();
        isReloading = false;
    }


    public void updateNetworkPosition()
    {
        if (grabbingWall)
        {
            transform.position = Vector3.Slerp(transform.position, m_networkedPosition, 15 * Time.deltaTime);
            return;
        }

        Vector3 dir = m_networkedPosition - transform.position;
        if (Mathf.Abs(dir.magnitude) > 5f)
        {
            transform.position = m_networkedPosition;
            dir = Vector3.zero;
            updateNetworkRotation();
            return;
        }

        //dir.y = 0;

        if (Mathf.Abs(dir.magnitude) < 0.02f)
        {
            _x = 0;
            _z = 0;
        }
        else
        {
            _x = dir.normalized.x;
            _z = dir.normalized.z;
        }
        jump = m_networkedPosition.y - transform.position.y > 0.2f;
        _rb.velocity = new Vector3(_rb.velocity.x, dir.normalized.y * 7, _rb.velocity.z);

        if (jump)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 10.5f, _rb.velocity.z);
            //_rb.velocity += dir.normalized.y * 10.5f;
        }
    }

    //public void updateNetworkPosition()
    //{
    //    //float ping = (float)PhotonNetwork.GetPing() * 0.001f;
    //    //float timeSinceLastUpdate = (float)(PhotonNetwork.Time - m_timePacketSent);
    //    //float totalTimePassed = ping + timeSinceLastUpdate;

    //    //Vector3 dir = m_networkedPosition - transform.position;

    //    //Vector3 extraPolatedPosition = m_networkedPosition + dir.normalized * 15 * totalTimePassed;

    //    //Vector3 newPos = Vector3.Lerp(transform.position, extraPolatedPosition, 15 * Time.deltaTime);
    //    //Debug.Log(transform.position + " " + extraPolatedPosition);

    //    //if (Vector3.Distance(transform.position, extraPolatedPosition) > 2f)
    //    //{
    //    //    newPos = extraPolatedPosition;
    //    //}

    //    //transform.position = newPos;

    //    //float time = m_timePacketSent - lastTimePacketSent;
    //    //currTimer += Time.deltaTime;
    //    //transform.position = Vector3.Lerp(lastNetworkedPosition, m_networkedPosition, PhotonNetwork.GetPing() * currTimer / time);
    //}

    public void updateNetworkRotation()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, m_networkedRotation, 180 * Time.deltaTime);
    }

    public void fixedUpdateNetworkPosition()
    {
        //if (grabbingWall) return;

        //_rb.velocity = new Vector3(_x * _currSpeed, _rb.velocity.y, _z * _currSpeed);

        //if (!isJumpDashing)
        //{
        //    Vector3 gravi = gravity * Vector3.up;
        //    _rb.AddForce(-gravi * Time.fixedDeltaTime, ForceMode.Acceleration);

        //}
        //else
        //{
        //    _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        //}
    }

    public void updateNetworkAnims()
    {
        if(m_isGrabbingWep) anim.SetTrigger("GrabWeapon");
        if(isHolsteringWep) anim.SetTrigger("Holster");
        if(isReloading) anim.SetTrigger("Reload");
    }

    [PunRPC]
    void updateVelocity(Vector3 v, Vector3 p)
    {
        if (photonView.IsMine) return;

        currentSnapshot = new Snapshot(transform.position, _rb.velocity, p, v, 1 / 20f);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(m_isGrabbingWep);
            stream.SendNext(isHolsteringWep);
            stream.SendNext(isReloading);

            //stream.SendNext(_currSpeed);
            //stream.SendNext(gravity);
            //stream.SendNext(grabbingWall);
            //stream.SendNext(isJumpDashing);
        }
        else
        {
            m_networkedPosition = (Vector3)stream.ReceiveNext();
            m_networkedRotation = (Quaternion)stream.ReceiveNext();
            m_isGrabbingWep = (bool)stream.ReceiveNext();
            isHolsteringWep = (bool)stream.ReceiveNext();
            isReloading = (bool)stream.ReceiveNext();

            

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
