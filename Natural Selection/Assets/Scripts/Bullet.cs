using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject bulletHole;


    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 0.25f);
        //Debug.DrawRay(ray.origin, ray.direction * 0.25f, Color.cyan);
        if(hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        //if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        //{
        //    Destroy(gameObject);
        //}
        if (!other.CompareTag("Player"))
        {
            //Debug.Log("Player hit");
            Destroy(gameObject);
        }
    }
}
