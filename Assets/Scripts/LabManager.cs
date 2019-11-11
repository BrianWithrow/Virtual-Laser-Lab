using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabManager : MonoBehaviour
{
    public RefractableMaterial rm;

    public GameObject rmSpawn;
    public GameObject obj;

    private List<RefractableMaterial> rms;

    private LaserController laser;

    private bool drag;

    // Start is called before the first frame update
    void Start()
    {
        rm = GetComponent<RefractableMaterial>();
        laser = FindObjectOfType<LaserController>();

        rm.SetPresetRefraction(RefractableMaterial.IndexesOfRefraction.AIR);

        rms = new List<RefractableMaterial>();

        foreach (RefractableMaterial rm in FindObjectsOfType<RefractableMaterial>())
        {
            rms.Add(rm);
        }

        rms.Remove(rm);

        drag = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hitInfo = new RaycastHit();

            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (hit)
            {
                if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Selectable"))
                {
                    if (obj == null || hitInfo.transform != obj.transform)
                    {
                        obj = hitInfo.transform.gameObject;
                    }
                
                    drag = true;
                }
            }
        }

        if (drag)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 13.2f);

            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);

            obj.transform.position = curPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            drag = false;
        }
    }

    public void RemoveObject()
    {
        if (rms.Count > 0)
        {
            RefractableMaterial removeRM = rms[rms.Count - 1];

            if (obj == removeRM.gameObject)
            {
                obj = laser.gameObject;
            }

            rms.Remove(removeRM);
            Destroy(removeRM.gameObject);
        }
    }

    public void AddObject()
    {
        if (rms.Count < 10)
        {
            GameObject spawn = Instantiate(rmSpawn) as GameObject;

            rms.Add(spawn.GetComponent<RefractableMaterial>());
        }
    }

    public void ResetWorldText()
    {
        foreach (RefractableMaterial rm in rms)
        {
            rm.text.text = "";
        }
    }

    public int ObjectCount()
    {
        return rms.Count;
    }
}
