using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class NetworkController : MonoBehaviourPun
{
    //struct Snapshot
    //{
    //    readonly Vector3 positionOnArrival;
    //    readonly Vector3 velocityOnArrival;
    //    readonly Vector3 reportedPosition;
    //    readonly Vector3 reportedVelocity;
    //    // Using floats for time works well for games that can go for hours.
    //    // Switch to doubles or other formats for games expected to run persistently.
    //    readonly float arrivalTime;
    //    readonly float blendWindow;

    //    public Snapshot(Vector3 currentPosition, Vector3 currentVelocity,
    //                   Vector3 reportedPosition, Vector3 reportedVelocity,
    //                   float blendWindow)
    //    {
    //        arrivalTime = Time.time;
    //        positionOnArrival = currentPosition;
    //        velocityOnArrival = currentVelocity;
    //        this.reportedPosition = reportedPosition;
    //        this.reportedVelocity = reportedVelocity;
    //        this.blendWindow = blendWindow;
    //    }

    //    public Vector3 EstimatePosition(float time, out Vector3 velocity)
    //    {
    //        float dT = time - arrivalTime;
    //        float blend = Mathf.Clamp01(dT / blendWindow);

    //        velocity = Vector3.Lerp(velocityOnArrival, reportedVelocity, blend);

    //        Vector3 position = Vector3.Lerp(
    //                   positionOnArrival + velocity * dT,
    //                   reportedPosition + reportedVelocity * dT,
    //                   blend);

    //        return position;
    //    }
    //}

    [HideInInspector]
    public Vector3 m_networkedPosition;
    [HideInInspector]
    public Vector3 lastNetworkedPosition;
    [HideInInspector]
    public Vector3 m_networkedVelocity;
    [HideInInspector]
    public Quaternion m_networkedRotation;
    [HideInInspector]
    public float m_timePacketSent;
    [HideInInspector]
    public float lastTimePacketSent;
    [HideInInspector]
    public float currTimer = 0;

    private PlayerController player;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine && PhotonNetwork.IsConnected) return;

        player = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && PhotonNetwork.IsConnected) return;

        //float dT = Time.time - m_timePacketSent;
        //float blend = Mathf.Clamp01(dT / (1 / 20f));

        //_rb.velocity = Vector3.Lerp(_rb.velocity, m_networkedVelocity, blend * Time.deltaTime);

        //transform.position = Vector3.Lerp(
        //           transform.position + _rb.velocity * dT,
        //           m_networkedPosition + m_networkedVelocity * dT,
        //           blend * Time.deltaTime);


        transform.position = Vector3.Slerp(transform.position, m_networkedPosition, 15 * Time.deltaTime);
        //transform.position = currentSnapshot.EstimatePosition(Time.time, out Vector3 velocity);
        //_rb.velocity = velocity;
        //Debug.Log(_rb.velocity);
        //updateNetworkPosition();
        //updateNetworkPosition();
        //Debug.Log(GrabWeaponBehaviour.isRifleUp);
        //updateNetworkAnims();
        updateNetworkRotation();
    }

    //public void updateNetworkPosition()
    //{
    //    if (grabbingWall)
    //    {
    //        transform.position = Vector3.Slerp(transform.position, m_networkedPosition, 15 * Time.deltaTime);
    //        return;
    //    }

    //    Vector3 dir = m_networkedPosition - transform.position;
    //    if (Mathf.Abs(dir.magnitude) > 5f)
    //    {
    //        transform.position = m_networkedPosition;
    //        dir = Vector3.zero;
    //        updateNetworkRotation();
    //        return;
    //    }

    //    //dir.y = 0;

    //    if (Mathf.Abs(dir.magnitude) < 0.02f)
    //    {
    //        _x = 0;
    //        _z = 0;
    //    }
    //    else
    //    {
    //        _x = dir.normalized.x;
    //        _z = dir.normalized.z;
    //    }
    //    jump = m_networkedPosition.y - transform.position.y > 0.2f;
    //    _rb.velocity = new Vector3(_rb.velocity.x, dir.normalized.y * 7, _rb.velocity.z);

    //    if (jump)
    //    {
    //        _rb.velocity = new Vector3(_rb.velocity.x, 10.5f, _rb.velocity.z);
    //        //_rb.velocity += dir.normalized.y * 10.5f;
    //    }
    //}

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
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, m_networkedRotation, 180 * Time.deltaTime);
        transform.rotation = m_networkedRotation;
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
        if (player.m_isGrabbingWep)
        {
            player.m_isGrabbingWep = false;
            anim.SetTrigger("GrabWeapon");
        }
        if (player.isHolsteringWep)
        {
            player.isHolsteringWep = false;
            anim.SetTrigger("Holster");
        }
        if (player.isReloading)
        {
            player.isReloading = false;
            anim.SetTrigger("Reload");
        }
    }
}
