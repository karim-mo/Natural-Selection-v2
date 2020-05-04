using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uselessgm : MonoBehaviour
{
    public GameObject prefab;
    // Start is called before the first frame update
    void Awake()
    {
        GameObject cube = Instantiate(prefab, prefab.transform.position, Quaternion.identity);
        
        GameObject _cube = Instantiate(prefab, prefab.transform.position, Quaternion.identity);
        cube.GetComponent<CubeTest>().speed = 7;
        cube.GetComponent<Rigidbody>().useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
