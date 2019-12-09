/*
 * This class contains user credentials used throughout the lab.
 */

using UnityEngine;

public class User : MonoBehaviour
{
    public string username;
    public string password;

    public static User instance;
    public bool guest;

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
