using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject usernameField;
    public GameObject passwordField;

    public GameObject usernameFieldSignIn;
    public GameObject passwordFieldSignIn;

    public GameObject saveButton;
    public GameObject loadButton;

    public Text validationText;
    public Text logInValidation;

    public string username;
    public string password;

    public User user;

    public bool checkingUser;

    void Awake()
    {
        //As the lab begins it will allow only one user to have its credentials filled.
        user = FindObjectOfType<User>();
    }

    public void SignIn()
    {
        //Sign in cant be called directly by button, so we call this function to call the sign in IEnumerator.
        StartCoroutine(CheckSignIn());
    }

    public IEnumerator CheckSignIn()
    {
        checkingUser = true;

        //Takes in user fields to check with the database that it works
        username = usernameField.GetComponent<InputField>().text;
        password = passwordField.GetComponent<InputField>().text;

        //Calls the php script to check user and pass.
        string getInstanceURL = "http://localhost/checkuserandpass.php?"; //be sure to add a ? to your url
        string get_url = getInstanceURL + "username=" + username + "&password=" + password;
        // Post the URL to the site and create a download object to get the result.
        UnityWebRequest hs_get = UnityWebRequest.Get(get_url);

        yield return hs_get.SendWebRequest();

        //Will check for an error with calling the script.
        if (hs_get.error != null)
        {
            Debug.Log("There was an error posting the high score: " + hs_get.error);
        }
        else
        {
            //Sign in check.
            if (hs_get.downloadHandler.text == "false")
            {
                //Account doesn't exist.
                logInValidation.text = "The username/password does not exist.";
            }
            else
            {
                //User object takes in fields as the lab switches.
                user.username = username;
                user.password = password;

                //Account exists.
                PlayGameUser();
            }
        }

        checkingUser = false;
    }

    public void PlayGameGuest()
    {
        //If using a guest account, the lab will run without saved instances.
        user.guest = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PlayGameUser()
    {
        //Will allow the user to enter the lab with the option to save/load instances.
        user.guest = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void toMainMenu()
    {
        //Allows users to exit to the main menu to sign in again or whatnot.
        SceneManager.LoadScene(0);
    }

    public void loadOptions()
    {
        if (!user.guest)
        {
            //Allows the option to save and load depending on if the user is a guest or not.
            saveButton.SetActive(true);
            loadButton.SetActive(true);
            
        }
    }

    public void setValues()
    {
        //Checks to ensure that the user input necessary fields to create an account.
        if (!checkingUser)
        {
            if (usernameFieldSignIn.GetComponent<InputField>().text == "" || passwordFieldSignIn.GetComponent<InputField>().text == "")
            {
                //The user did not enter any text into the user/password boxes.
                validationText.text = "Please insert a Username/Password.";
            }
            else
            {
                //The database will be checked to confirm the user.
                StartCoroutine(CheckUser());
            }
        }
    }

    public IEnumerator CheckUser()
    {
        checkingUser = true;

        //Gets the username and password from the input fields.
        username = usernameFieldSignIn.GetComponent<InputField>().text;
        password = passwordFieldSignIn.GetComponent<InputField>().text;

        //Calling the php script
        string getInstanceURL = "http://localhost/checkuser.php?"; //be sure to add a ? to your url
        string get_url = getInstanceURL + "username=" + username;
        // Post the URL to the site and create a download object to get the result.
        UnityWebRequest hs_get = UnityWebRequest.Get(get_url);

        yield return hs_get.SendWebRequest();

        //Checks for errors within php script.
        if (hs_get.error != null)
        {
            Debug.Log("There was an error posting the high score: " + hs_get.error);
        }
        else
        {
            //If all is well, the php script for adding a user will be called.
            if (hs_get.downloadHandler.text == "false")
            {
                string secretKey = "virtual-laser-lab"; // Edit this value and make sure it's the same as the one stored on the server
                string updateUserURL = "http://localhost/adduser.php?"; //be sure to add a ? to your url

                //This connects to a server side php script that will add the name and score to a MySQL DB.
                // Supply it with a string representing the players name and the players score.
                string hash = LabManager.Md5Sum(username + password + secretKey);

                string post_url = updateUserURL + "username=" + UnityWebRequest.EscapeURL(username) + "&password=" + password + "&hash=" + hash;

                // Post the URL to the site and create a download object to get the result.
                hs_get = UnityWebRequest.Get(post_url);

                yield return hs_get.SendWebRequest();

                if (hs_get.error != null)
                {
                    Debug.Log("There was an error posting the high score: " + hs_get.error);
                }
                else
                    validationText.text = "Account created.";

            }
            else
            {
                //The user already exists in the system.
                validationText.text = "User already exists.";
            }
        }

        checkingUser = false;
    }
}
