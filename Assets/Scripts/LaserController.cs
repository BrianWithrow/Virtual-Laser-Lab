/*
 * This class is the laser controller of the lab, handles simulating
 * Snell's law, as well as rotating through user input
 */

using System;
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

    private LineRenderer lr;
    private LabManager lm;
    
    private float degrees1;
    private float degrees2;

	// initialize fields
	void Start () {
        lr = GetComponent<LineRenderer>();
        lm = FindObjectOfType<LabManager>();
	}
	
	void Update () {
        Rotate();

        lm.ResetWorldText();

        lr.positionCount = 2;

        RayOut(transform.position, transform.forward, 0);
    }

    /*
     * rotate laser through input
     */
    public void Rotate()
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

    /*
     * takes in n degrees and convert to radians
     */
    public float toRadians(float n)
    {
        n = n * Mathf.PI / 180;
        return n;
    }

    /*
     * takes in n radians and return degrees
     */
    public float toDegrees(float n)
    {
        n = n *  180 / Mathf.PI;
        return n;
    }

    /*
     * takes in n1, n2, and degrees1. n is the index of refraction, degrees1 is the angle of entry.
     * returns refractOut, the angle of exit into the new material.
     */
    public float snellCalculator(float n1, float n2, float degrees1) {
        float refractOut;

        refractOut = Mathf.Asin(n1 * Mathf.Sin(toRadians(degrees1)) / n2);

        return refractOut;
    }

    public void RayOut(Vector3 origin, Vector3 direction, int currentLinePosition)
    {
        RaycastHit hit;
        Ray ray = new Ray();

        ray.origin = origin;
        ray.direction = direction;

        lr.SetPosition(currentLinePosition++, ray.origin);

        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if (hit.transform.gameObject.tag == "Material")
            {
                RefractableMaterial rm = hit.transform.gameObject.GetComponent<RefractableMaterial>();

                lr.positionCount = currentLinePosition + 4;

                degrees1 = 180 - Vector3.SignedAngle(ray.direction, hit.normal, Vector3.up);
                degrees2 = toDegrees(snellCalculator(lm.rm.GetIndexOfRefraction(), rm.GetIndexOfRefraction(), degrees1));

                rm.text.text = 
                    "θ1: " + String.Format("{0:0.000}", degrees1) + "\n" + 
                    "θ2: " + String.Format("{0:0.000}", degrees2);

                // render line from laser to point of contact with material
                lr.SetPosition(currentLinePosition++, hit.point);

                RaycastHit hitExit;
                Ray rayExit = new Ray();

                rayExit.origin = hit.point - ((Quaternion.AngleAxis(degrees2, Vector3.up) * hit.normal).normalized * 1.5f);
                rayExit.direction = (Quaternion.AngleAxis(degrees2, Vector3.up) * hit.normal).normalized;

                if (Physics.Raycast(rayExit, out hitExit, 10.0f))
                {
                    if (hitExit.transform == hit.transform)
                    {
                        degrees1 = 180 - Vector3.SignedAngle(rayExit.direction, hitExit.normal, Vector3.up);
                        degrees2 = toDegrees(snellCalculator(rm.GetIndexOfRefraction(), lm.rm.GetIndexOfRefraction(), degrees1));

                        // render line from contact point to exit point
                        lr.SetPosition(currentLinePosition++, hitExit.point);

                        RayOut(hitExit.point, direction, currentLinePosition);
                    }
                }
            }
        }
        else
        {
            lr.SetPosition(currentLinePosition, origin + (direction * 100.0f));
        }
    }

}
