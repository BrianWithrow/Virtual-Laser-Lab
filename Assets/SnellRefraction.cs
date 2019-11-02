using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnellRefraction : MonoBehaviour {

    private LineRenderer lr;
    private Prism prism;
    public float n1;
    public float n2;
    public float degrees1;
    public float degrees2;

    public bool direction;

	// Use this for initialization
	void Start () {
        lr = GetComponent<LineRenderer>();
        n1 = 1f;
        n2 = 2.42f;

	}
	
	// Update is called once per frame
	void Update () {
        //RaycastHit hit;
        //Ray ray = new Ray();
        //
        //ray.origin = transform.position + transform.forward;
        //ray.direction = transform.forward;
        //
        //lr.SetPosition(0, ray.origin);
        //
        //if (Physics.Raycast(ray, out hit, 10.0f))
        //{
        //    lr.positionCount = 4;
        //
        //    if (hit.transform.gameObject.tag == "Material")
        //    {
        //        degrees1 = 180 - Vector3.SignedAngle(ray.direction, hit.normal, Vector3.up);
        //        degrees2 = toDegrees(snellCalculator(n1, n2, degrees1));
        //        
        //        // render line from laser to point of contact with material
        //        lr.SetPosition(1, hit.point);
        //
        //        RaycastHit hitExit;
        //        Ray rayExit = new Ray();
        //
        //        rayExit.origin = hit.point - ((Quaternion.AngleAxis(degrees2, Vector3.up) * hit.normal).normalized * 10.0f);
        //        rayExit.direction = (Quaternion.AngleAxis(degrees2, Vector3.up) * hit.normal).normalized;
        //
        //        if (Physics.Raycast(rayExit, out hitExit, 10.0f))
        //        {
        //            if (hitExit.transform == hit.transform)
        //            {
        //                degrees1 = 180 - Vector3.SignedAngle(rayExit.direction, hitExit.normal, Vector3.up);
        //                degrees2 = toDegrees(snellCalculator(n2, n1, degrees1));
        //
        //                // render line from contact point to exit point
        //                lr.SetPosition(2, hitExit.point);
        //
        //                // render line from exit point out
        //                lr.SetPosition(3, hitExit.point + ((Quaternion.AngleAxis(degrees2, Vector3.up) * hitExit.normal).normalized * 100.0f));
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    lr.positionCount = 2;
        //    lr.SetPosition(1, (ray.direction) * 100.0f);
        //}

        lr.positionCount = 2;

        RayOut(transform.position + transform.forward, transform.forward, 0);
    }

    //Function will take in n degrees and convert to radians
    public float toRadians(float n)
    {
        n = n * Mathf.PI / 180;
        return n;
    }

    //Function will take in n radians and return degrees
    public float toDegrees(float n)
    {
        n = n *  180 / Mathf.PI;
        return n;
    }

    //Function will take in n1, n2, and degrees1. n is the index of refraction, degrees1 is the angle of entry.
    //Returns refractOut, the angle of exit into the new material.
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
                lr.positionCount = currentLinePosition + 4;

                degrees1 = 180 - Vector3.SignedAngle(ray.direction, hit.normal, Vector3.up);
                degrees2 = toDegrees(snellCalculator(n1, n2, degrees1));
                
                // render line from laser to point of contact with material
                lr.SetPosition(currentLinePosition++, hit.point);

                RaycastHit hitExit;
                Ray rayExit = new Ray();

                rayExit.origin = hit.point - ((Quaternion.AngleAxis(degrees2, Vector3.up) * hit.normal).normalized * 1.1f);
                rayExit.direction = (Quaternion.AngleAxis(degrees2, Vector3.up) * hit.normal).normalized;

                if (Physics.Raycast(rayExit, out hitExit, 10.0f))
                {
                    if (hitExit.transform == hit.transform)
                    {
                        degrees1 = 180 - Vector3.SignedAngle(rayExit.direction, hitExit.normal, Vector3.up);
                        degrees2 = toDegrees(snellCalculator(n2, n1, degrees1));

                        // render line from contact point to exit point
                        lr.SetPosition(currentLinePosition++, hitExit.point);

                        RayOut(hitExit.point, direction, currentLinePosition);
                    }
                }
            }
        }
        else
        {
            lr.SetPosition(currentLinePosition, (ray.direction) * 100.0f);
        }
    }

}
