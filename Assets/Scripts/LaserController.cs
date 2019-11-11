using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    public float rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") > 0.0f)
        {
            transform.Rotate(new Vector3(0.0f, rotationSpeed * Time.deltaTime, 0.0f));
        }
        else if (Input.GetAxisRaw("Horizontal") < 0.0f)
        {
            transform.Rotate(new Vector3(0.0f, -rotationSpeed * Time.deltaTime, 0.0f));
        }
    }
}
