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

    public void checkSignIn()
    {
        username = usernameField.GetComponent<Text>().text;
        password = passwordField.GetComponent<Text>().text;

    }

    public void PlayGame()
    {
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
}
