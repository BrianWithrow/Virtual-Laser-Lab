using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public RefractableMaterial rm;

    public GameObject rmSpawn;
    public GameObject obj;

    private List<RefractableMaterial> rms;

    private LaserController laser;

    private bool drag;

    public bool save;
    public bool loading;

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

        Load();
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

    public void Save()
    {
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

        string json = JsonUtility.ToJson(li);

        User user = FindObjectOfType<User>();

        StartCoroutine(UpdateUser(user.username, user.password, json));
    }

    public void Load()
    {
        User user = FindObjectOfType<User>();

        if (!user.guest)
        {
            StartCoroutine(LoadInstance(user.username));
        }
    }

    IEnumerator UpdateUser(string username, string password, string instance)
    {
        string secretKey = "virtual-laser-lab";
        string updateUserURL = "http://localhost/updateuser.php?";
        
        string hash = Md5Sum(username + password + secretKey);

        string get_url = updateUserURL + "username=" + UnityWebRequest.EscapeURL(username) + "&password=" + password + "&instance=" + instance + "&hash=" + hash;

        Debug.Log(get_url);
        
        UnityWebRequest hs_post = UnityWebRequest.Get(get_url);

        Debug.Log("Saving");

        yield return hs_post.SendWebRequest();

        Debug.Log("Done saving");

        if (hs_post.error != null)
        {
            Debug.Log("There was an error posting the high score: " + hs_post.error);
        }
        else
        {
            Debug.Log(hs_post.downloadHandler.text);
        }
    }

    IEnumerator LoadInstance(string username)
    {
        string getInstanceURL = "http://localhost/loadinstance.php?"; //be sure to add a ? to your url
        string get_url = getInstanceURL + "username=" + username;
        // Post the URL to the site and create a download object to get the result.
        UnityWebRequest hs_get = UnityWebRequest.Get(get_url); ;

        Debug.Log("Loading");

        yield return hs_get.SendWebRequest();

        Debug.Log("Done loading");

        if (hs_get.error != null)
        {
            Debug.Log("There was an error posting the high score: " + hs_get.error);
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

    public void SetUpWorld(string json)
    {
        LabInstance li = JsonUtility.FromJson<LabInstance>(json);

        laser.transform.position = li.lm.pos;
        laser.transform.rotation = li.lm.rot;

        rm.SetPresetRefraction((RefractableMaterial.IndexesOfRefraction)Enum.ToObject(typeof(RefractableMaterial.IndexesOfRefraction), li.worldRM.presetRefraction));
        rm.SetCustomRefraction(li.worldRM.n);

        int x = 0;

        foreach (RefractableMaterial rm in rms)
        {
            rm.transform.position = li.refractableMaterials.materials[x].pos;
            rm.transform.rotation = li.refractableMaterials.materials[x].rot;
            rm.SetPresetRefraction((RefractableMaterial.IndexesOfRefraction)Enum.ToObject(typeof(RefractableMaterial.IndexesOfRefraction), li.refractableMaterials.materials[x].presetRefraction));
            rm.SetCustomRefraction(li.refractableMaterials.materials[x].n);

            x++;
        }

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
