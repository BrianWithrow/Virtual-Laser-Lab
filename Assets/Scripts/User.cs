/*
 * This class contains user credentials used throughout the lab.
 */

using UnityEngine;

public class User : MonoBehaviour
{
    public string username;         // username of the current user
    public string password;         // password of the current user

    public static User instance;    // instance to keep user object active in between scenes
    public bool guest;              // checks if current user is a guest

    /*
     * keeps the same user object throughout scenes
     */
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
