using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector3 offset;

    public float peakSpeed;
    public float mainSpeed;
    public float jumpForce;

    public float gravity;

    public CameraController camBase;
    public LayerMask jumpableLayers;
    [HideInInspector]
    public bool jump;

    private float _currSpeed;
    private Rigidbody _rb;
    private Animator anim;
    private CapsuleCollider col;
    private RaycastHit _ground;
    private Vector3 _groundLoc;
    private Camera cam;
    private bool LShift = false;
    private float _x;
    private float _z;
    Vector3 targetPos;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        camBase = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        cam = camBase.gameObject.transform.GetChild(0).gameObject.GetComponent<Camera>();
        col = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        //Debug.Log(Input.GetKey(KeyCode.W));

        
        _x = Mathf.Clamp(Input.GetAxis("Horizontal") * 2, -1, 1);
        _z = Mathf.Clamp(Input.GetAxis("Vertical") * 2, -1, 1);
        anim.SetFloat("VelX", _x);
        anim.SetFloat("VelZ", _z);

        _x = Input.GetAxisRaw("Horizontal");
        _z = Input.GetAxisRaw("Vertical");


        //transform.rotation = cam.gameObject.transform.rotation;
        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, camBase.gameObject.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        transform.rotation = rot;

        #region Shooting testing
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(ray.origin, ray.direction * 40f, Color.red);
        RaycastHit hit;
        
        if(Physics.Raycast(ray, out hit, 40f))
        {
            //Debug.Log(hit.point + " " + hit.collider.gameObject.name);
            targetPos = hit.point;
        }
        else
        {
            //Debug.Log(ray.GetPoint(20f));
            targetPos = ray.GetPoint(20f);
            
        }

        
        #endregion

        _currSpeed = LShift ? mainSpeed : mainSpeed;



        jump = Input.GetButtonDown("Jump");
        //Debug.Log(jump);
        
        bool grounded = isGrounded();

        //if (_x == 0 && _z == 0 && grounded) _rb.velocity = Vector3.zero;

        if (isGrounded() && jump)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.velocity += Vector3.up * jumpForce;
        }


        anim.SetBool("Grounded", grounded);
        //Debug.Log(_z);

        if (_rb.velocity.y > (0 + 1.5f))
        {
            anim.SetInteger("state", 2);
        }
        else if (_rb.velocity.y < (0 - 1.5f) && !grounded)
        {
            anim.SetInteger("state", 1);
        }
        else anim.SetInteger("state", 0);

        //Debug.Log(_rb.velocity.y);
        //if (Input.GetButtonDown("Jump")) EditorApplication.isPaused = true;

    }

    void FixedUpdate()
    {
        Vector3 mov = transform.position;
        //_rb.MovePosition(transform.position + Time.deltaTime * _currSpeed * transform.TransformDirection(_x, 0, _z));
        //float velY = _rb.velocity.y;
        float finalSpeedX = _currSpeed * _x;
        float finalSpeedZ = _currSpeed * _z;

        if(Mathf.Abs(_x ) > 0 && Mathf.Abs(_z) > 0)
        {
            finalSpeedX *= 0.5f;
            finalSpeedZ *= 0.5f;
        }

        _rb.velocity = transform.TransformDirection(finalSpeedX, _rb.velocity.y, finalSpeedZ);

        //_rb.velocity = new Vector3(_rb.velocity.x, velY, _rb.velocity.z);
        //new Vector3(_currSpeed * _x, 0, _currSpeed * _z);

        Vector3 grav = -gravity * Vector3.up;
        _rb.AddForce(grav * Time.fixedDeltaTime, ForceMode.Acceleration);

        


        //transform.position = mov;
    }

    public bool isGrounded()
    {
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z), col.radius * .4f, jumpableLayers);
    }
    private void LateUpdate()
    {
        //Transform rightElbow = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);

        //rightElbow.LookAt(targetPos);
        //rightElbow.rotation = rightElbow.rotation * Quaternion.Euler(offset);
        //rightElbow.Rotate(90, 0, 0, Space.Self);



        //Transform rightHand = anim.GetBoneTransform(HumanBodyBones.RightHand);
        //rightHand.Rotate(0, -45, 0, Space.Self);

        //Transform leftElbow = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);

        //leftElbow.LookAt(rightHand);
        //leftElbow.rotation = leftElbow.rotation * Quaternion.Euler(offset);
        //leftElbow.Rotate(90, 0, 0, Space.Self);

        //Transform leftHand = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        //leftHand.Rotate(0, 90, 0, Space.Self);

        Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
        //chest.LookAt(new Vector3(targetPos.x - offset.x, targetPos.y, targetPos.z));
        chest.LookAt(targetPos);
        chest.Rotate(10, 45, 0, Space.Self);
    }
    void OnAnimatorIK(int layerIndex)
    {

        //anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        //anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0.5f);
        //anim.SetIKPosition(AvatarIKGoal.RightHand, elbow.transform.position); // Crosshair target position in world coords
        //anim.SetIKHintPosition(AvatarIKHint.RightElbow, elbow.transform.position); // Elbow bone

        //anim.SetLookAtWeight(1);
        //anim.SetLookAtPosition(targetPos);
    }


}
