using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTest : MonoBehaviour
{
    public float speed;

    private Rigidbody _rb;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Random.Range(0, 10) + gameObject.name);
        float z = Input.GetAxisRaw("Vertical");

        _rb.velocity = transform.TransformDirection(new Vector3(0, 0, z * speed));


    }
}
