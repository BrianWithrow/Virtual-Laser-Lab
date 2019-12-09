/*
 * This class handles saving, loading lab instances, 
 * as well as handling mouse inputs from users.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LabInstance
{
    public LaserModel lm;

    public RefractableMaterialModel worldRM;

    public RefractableMaterials refractableMaterials;
}

public class LabManager : MonoBehaviour
{
    private List<RefractableMaterial> rms;      // refractable materials in lab scene, excluding rm
    public RefractableMaterial rm;              // this is the lab scene's refractable material, pretty much air's index of refraction

    public GameObject rmSpawn;                  // refractable material to spawn
    public GameObject obj;                      // object clicked by user

    private LaserController laser;              // laser object in lab scene

    private bool drag;                          // used to check if an object is being dragged by mouse
    
    public string defaultInstance;              // json string of default instance

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

        defaultInstance = GenerateJSON();

        Load();
    }

    // Update is called once per frame
    void Update()
    {
        // left moust button down
        if (Input.GetMouseButton(0))
        {
            // check if a selectable object is clicked
            RaycastHit hitInfo = new RaycastHit();

            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (hit)
            {
                if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Selectable"))
                {
                    // set obj to selected object
                    if (obj == null || hitInfo.transform != obj.transform)
                    {
                        obj = hitInfo.transform.gameObject;
                    }
                
                    drag = true;
                }
            }
        }

        // if dragging object
        if (drag)
        {
            // move object with mouse
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 13.2f);

            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);

            obj.transform.position = curPosition;
        }

        // if left mouse button is released, stop dragging object
        if (Input.GetMouseButtonUp(0))
        {
            drag = false;
        }
    }

    /*
     * removes object from lab
     */
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

    /*
     * add object to lab
     */
    public RefractableMaterial AddObject()
    {
        if (rms.Count < 10)
        {
            GameObject spawn = Instantiate(rmSpawn) as GameObject;

            rms.Add(spawn.GetComponent<RefractableMaterial>());

            return spawn.GetComponent<RefractableMaterial>();
        }
        else
            return null;
    }

    /*
     * set world text in refractable objects to nothing
     */
    public void ResetWorldText()
    {
        foreach (RefractableMaterial rm in rms)
        {
            rm.text.text = "";
        }
    }

    /*
     * reset world to default instance
     */
    public void Reset()
    {
        while (rms.Count > 1)
        {
            RemoveObject();
        }

        SetUpWorld(defaultInstance);
    }

    /*
     * get count of objects in world
     */
    public int ObjectCount()
    {
        return rms.Count;
    }

    /*
     * save lab instance
     */
    public void Save()
    {
        string json = GenerateJSON();

        User user = FindObjectOfType<User>();

        StartCoroutine(UpdateUser(user.username, user.password, json));
    }

    /*
     * load lab instance
     */
    public void Load()
    {
        User user = FindObjectOfType<User>();

        if (!user.guest)
        {
            StartCoroutine(LoadInstance(user.username));
        }
    }

    /*
     * updates user's saved lab instance
     */
    IEnumerator UpdateUser(string username, string password, string instance)
    {
        string secretKey = "virtual-laser-lab";
        string updateUserURL = "http://69.88.163.18/virtuallaserlab/updateuser.php?";
        
        string hash = Md5Sum(username + password + secretKey);

        string get_url = updateUserURL + "username=" + UnityWebRequest.EscapeURL(username) + "&password=" + password + "&instance=" + instance + "&hash=" + hash;

        Debug.Log(get_url);
        
        UnityWebRequest hs_post = UnityWebRequest.Get(get_url);

        Debug.Log("Saving");

        yield return hs_post.SendWebRequest();

        Debug.Log("Done saving");

        if (hs_post.error != null)
        {
            Debug.Log("There was an error updating user: " + hs_post.error);
        }
        else
        {
            Debug.Log(hs_post.downloadHandler.text);
        }
    }

    /*
     * load instance from user's account
     */
    IEnumerator LoadInstance(string username)
    {
        string getInstanceURL = "http://69.88.163.18/virtuallaserlab/loadinstance.php?";
        string get_url = getInstanceURL + "username=" + username;
        
        UnityWebRequest hs_get = UnityWebRequest.Get(get_url); ;

        Debug.Log("Loading");

        yield return hs_get.SendWebRequest();

        Debug.Log("Done loading");

        if (hs_get.error != null)
        {
            Debug.Log("There was an error loading instance: " + hs_get.error);
        }
        else
        {
            if (hs_get.downloadHandler.text != "")
            {
                Debug.Log(hs_get.downloadHandler.text);

                SetUpWorld(hs_get.downloadHandler.text);
            }
        }
    }

    /*
     * generate json object of lab instacne
     */
    public string GenerateJSON()
    {
        // create instances of objecct models
        LaserModel lm = new LaserModel(laser.transform.position, laser.transform.rotation);

        RefractableMaterialModel worldRM = new RefractableMaterialModel(rm.GetPresetIndex(), rm.GetN(), rm.transform.position, rm.transform.rotation);

        RefractableMaterials serializeRMs = new RefractableMaterials();

        foreach (RefractableMaterial rm in rms)
        {
            serializeRMs.materials.Add(new RefractableMaterialModel(rm.GetPresetIndex(), rm.GetN(), rm.transform.position, rm.transform.rotation));
        }

        LabInstance li = new LabInstance();

        li.lm = lm;
        li.worldRM = worldRM;
        li.refractableMaterials = serializeRMs;

        // convert lab instance model object to json string
        return JsonUtility.ToJson(li);
    }

    /*
     * set up world from json string
     */
    public void SetUpWorld(string json)
    {
        // generate lab instance model object from json string
        LabInstance li = JsonUtility.FromJson<LabInstance>(json);

        // set laser object from json
        laser.transform.position = li.lm.pos;
        laser.transform.rotation = li.lm.rot;

        // set lab manager's refratable material componenet from json
        rm.SetPresetRefraction((RefractableMaterial.IndexesOfRefraction)Enum.ToObject(typeof(RefractableMaterial.IndexesOfRefraction), li.worldRM.presetRefraction));
        rm.SetCustomRefraction(li.worldRM.n);

        int x = 0;
        
        // set available refratable materials 
        foreach (RefractableMaterial rm in rms)
        {
            rm.transform.position = li.refractableMaterials.materials[x].pos;
            rm.transform.rotation = li.refractableMaterials.materials[x].rot;
            rm.SetPresetRefraction((RefractableMaterial.IndexesOfRefraction)Enum.ToObject(typeof(RefractableMaterial.IndexesOfRefraction), li.refractableMaterials.materials[x].presetRefraction));
            rm.SetCustomRefraction(li.refractableMaterials.materials[x].n);

            x++;
        }

        // if there are more materials to generate, create new ones
        while (rms.Count < li.refractableMaterials.materials.Count)
        {
            RefractableMaterial temp = AddObject();

            temp.transform.position = li.refractableMaterials.materials[x].pos;
            temp.transform.rotation = li.refractableMaterials.materials[x].rot;
            temp.SetPresetRefraction((RefractableMaterial.IndexesOfRefraction)Enum.ToObject(typeof(RefractableMaterial.IndexesOfRefraction), li.refractableMaterials.materials[x].presetRefraction));

            Debug.Log(li.refractableMaterials.materials[x].presetRefraction);
            Debug.Log("PR: " + temp.GetPresetIndex());

            temp.SetCustomRefraction(li.refractableMaterials.materials[x].n);

            x++;
        }
    }

    /*
     * generates hash for sending request to php scripts in the web server
     */
    public static string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    
}
