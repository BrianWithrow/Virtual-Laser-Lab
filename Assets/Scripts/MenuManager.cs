using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    private string username;
    private string password;
    public GameObject usernameField;
    public GameObject passwordField;
    public bool guest;

    public static MenuManager instance;

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
    }

    public void checkSignIn()
    {
        username = usernameField.GetComponent<Text>().text;
        password = passwordField.GetComponent<Text>().text;
    }

    public void PlayGameGuest()
    {
        guest = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PlayGameUser()
    {
        Debug.Log("This worked");
        guest = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void toMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void loadOptions()
    {
        /*if (guest)
        {
            foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("UserOptions"))
            {
                gameObject.active = false;
            }
            
        }
        */
    }
}
