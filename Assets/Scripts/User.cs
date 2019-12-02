using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    public string username;
    public string password;

    public static User instance;
    public bool guest;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        
        DontDestroyOnLoad(gameObject);

        Debug.Log(username);
        Debug.Log(password);
    }
}
