using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LaserModel
{
    public Vector3 pos;
    public Quaternion rot;

    public LaserModel(Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }
}

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
