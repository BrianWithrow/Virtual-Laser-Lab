/*
 * This class is the laser controller of the lab, handles simulating
 * Snell's law, as well as rotating through user input
 */

using System;
using UnityEngine;

// model for serializing laser object to JSON
[Serializable]
public class LaserModel
{
    public Vector3 pos;
    public Quaternion rot;
    public Color color;

    public LaserModel(Vector3 pos, Quaternion rot, Color color)
    {
        this.pos = pos;
        this.rot = rot;
        this.color = color;
    }
}

public class LaserController : MonoBehaviour
{
    public float rotationSpeed;                 // rotating speed when using keyboard inputs

    private LineRenderer lr;                    // line renderer to visualize laser ray
    private LabManager lm;                      // lab manager instance

    public Color color;
    
    private float degrees1;
    private float degrees2;

	// initialize fields
	void Start () {
        lr = GetComponent<LineRenderer>();

        color = Color.white;

        lm = FindObjectOfType<LabManager>();
	}
	
	void Update () {
        Rotate();

        lm.ResetWorldText();

        lr.positionCount = 2;
        lr.material.color = color;

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

                degrees1 = 180 - Vector3.SignedAngle(ray.direction, hit.normal, Vector3.up);

                if (!rm.IsReflectable()){
                    lr.positionCount = currentLinePosition + 4;
                    degrees2 = toDegrees(snellCalculator(lm.rm.GetIndexOfRefraction(), rm.GetIndexOfRefraction(), degrees1));

                    rm.text.text = 
                        "θ1: " + String.Format("{0:0.000}", degrees1) + "\n" + 
                        "θ2: " + String.Format("{0:0.000}", degrees2);

                    // render line from laser to point of contact with material
                    lr.SetPosition(currentLinePosition++, hit.point);

                    RaycastHit hitExit;
                    Ray rayExit = new Ray();

                    rayExit.origin = hit.point - ((Quaternion.AngleAxis(degrees2, Vector3.up) * hit.normal).normalized * 1.5f * rm.transform.localScale.x);
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
                else 
                {
                    lr.positionCount = currentLinePosition + 2;
                    degrees2 = -degrees1;

                    rm.text.text = 
                        "θ1: " + String.Format("{0:0.000}", degrees1) + "\n" + 
                        "θ2: " + String.Format("{0:0.000}", degrees2);
                    
                    RayOut(hit.point, (Quaternion.AngleAxis(degrees2, Vector3.up) * hit.normal).normalized, currentLinePosition);
                }
            }
        }
        else
        {
            lr.SetPosition(currentLinePosition, origin + (direction * 100.0f));
        }
    }

}
