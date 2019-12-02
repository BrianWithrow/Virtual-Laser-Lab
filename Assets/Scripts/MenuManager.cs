using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject usernameField;
    public GameObject passwordField;

    public GameObject saveButton;
    public GameObject loadButton;

    public User user;

    void Awake()
    {
        Scene scene = SceneManager.GetActiveScene();
        user = FindObjectOfType<User>();

        if (scene.buildIndex == 1)
        {
            //foreach
        }
    }

    public void checkSignIn()
    {
        user.username = usernameField.GetComponent<InputField>().text;
        user.password = passwordField.GetComponent<InputField>().text;
    }

    public void PlayGameGuest()
    {
        user.guest = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PlayGameUser()
    {
        user.guest = false;
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
        if (!user.guest)
        {
            /*foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("UserButtons"))
            {
                
            }*/
            saveButton.SetActive(true);
            loadButton.SetActive(true);
            
        }
        else
        {

        }
    }
}
