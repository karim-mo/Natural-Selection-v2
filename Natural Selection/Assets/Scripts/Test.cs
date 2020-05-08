using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private Animator _animator;
    public float MaxSpeed = 10;
    public float NormalSpeed = 5;

    private float currentSpeed;
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (_animator == null) return;

        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _animator.SetBool("isRunning", true);
            //The audio source part
            currentSpeed = MaxSpeed;
        }
        else
        {
            _animator.SetBool("isRunning", false);
            currentSpeed = NormalSpeed;
        }
        Move(x, y); 
    }

    private void Move(float x, float y)
    {
        _animator.SetFloat("VelX", x);
        _animator.SetFloat("VelY", y);

        transform.position += transform.TransformDirection(x, 0, y) * currentSpeed * Time.deltaTime;
    }
}
