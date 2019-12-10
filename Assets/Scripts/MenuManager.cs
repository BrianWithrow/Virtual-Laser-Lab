/*
 * This class handles menu ui components.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject usernameField;            // username sign in field
    public GameObject passwordField;            // password sign in field

    public GameObject createUserUsernameField;  // create account username field
    public GameObject createUserPasswordField;  // create account password field

    public GameObject saveButton;               // lab scene save button
    public GameObject loadButton;               // lab scene load button

    public Text validationText;                 // text to notify users of their input in create account screen
    public Text logInValidation;                // text to notify users of their input in login screen

    public string username;                     // username string field
    public string password;                     // password string field

    public User user;                           // user instance

    public bool checkingUser;                   // bool to use when checking user input

    void Awake()
    {
        // as the lab begins it will allow only one user to have its credentials filled
        user = FindObjectOfType<User>();

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            passwordField.GetComponent<InputField>().inputType = InputField.InputType.Password;
            createUserPasswordField.GetComponent<InputField>().inputType = InputField.InputType.Password;
        }
    }

    /*
     * starts CheckSignIn routine
     */
    public void SignIn()
    {
        StartCoroutine(CheckSignIn());
    }

    /*
     * if using a guest account, the lab will run without saving and loading enabled
     */
    public void PlayGameGuest()
    {
        user.guest = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /*
     * if using an account, the lab will run with saving and loading enabled
     */
    public void PlayGameUser()
    {
        user.guest = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /*
     * go back to main menu
     */
    public void ToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    /*
     * enables save and load buttons in lab scene
     */
    public void LoadOptions()
    {
        if (!user.guest)
        {
            saveButton.SetActive(true);
            loadButton.SetActive(true);
        }
    }

    public void ValidateCreateAccountInput()
    {
        // checks to ensure that the user input necessary fields to create an account.
        if (!checkingUser)
        {
            // the user did not enter any text into the user/password boxes.
            if (createUserUsernameField.GetComponent<InputField>().text == "" || createUserPasswordField.GetComponent<InputField>().text == "")
            {
                validationText.text = "Please insert a Username/Password.";
            }
            else
            {
                // the database will be checked to confirm the user.
                StartCoroutine(CheckUser());
            }
        }
    }

    /*
     * checks if the database contains a row with the same username in the input field
     */
    public IEnumerator CheckUser()
    {
        checkingUser = true;
        
        username = createUserUsernameField.GetComponent<InputField>().text;
        password = createUserPasswordField.GetComponent<InputField>().text;

        // send get request to php script to check username and password
        string getInstanceURL = "http://69.88.163.18/virtuallaserlab/checkuser.php?";
        string get_url = getInstanceURL + "username=" + username;

        UnityWebRequest hs_get = UnityWebRequest.Get(get_url);

        yield return hs_get.SendWebRequest();

        // check for an error
        if (hs_get.error != null)
        {
            Debug.Log("There was an error checking user: " + hs_get.error);
        }
        else
        {
            // account doesn't exist
            if (hs_get.downloadHandler.text == "false")
            {
                string secretKey = "virtual-laser-lab";
                string updateUserURL = "http://69.88.163.18/virtuallaserlab/adduser.php?";
                
                // send hash
                string hash = LabManager.Md5Sum(username + password + secretKey);

                // send get request to php script to create account
                get_url = updateUserURL + "username=" + UnityWebRequest.EscapeURL(username) + "&password=" + password + "&hash=" + hash;
                
                hs_get = UnityWebRequest.Get(get_url);

                yield return hs_get.SendWebRequest();

                // check for an error
                if (hs_get.error != null)
                {
                    Debug.Log("There was an error creating account: " + hs_get.error);
                }
                else
                {
                    validationText.text = "Account created.";
                }
            }
            else
            {
                // the user already exists in the database
                validationText.text = "User already exists.";
            }
        }

        checkingUser = false;
    }

    /*
     * checks if the database contains a row with the same username and password entered in input fields 
     */
    public IEnumerator CheckSignIn()
    {
        checkingUser = true;

        username = usernameField.GetComponent<InputField>().text;
        password = passwordField.GetComponent<InputField>().text;

        // send get request to php script to check username and password
        string getInstanceURL = "http://69.88.163.18/virtuallaserlab/checkuserandpass.php?";
        string get_url = getInstanceURL + "username=" + username + "&password=" + password;

        UnityWebRequest hs_get = UnityWebRequest.Get(get_url);

        yield return hs_get.SendWebRequest();

        // check for an error
        if (hs_get.error != null)
        {
            Debug.Log("There was an error checking sign in: " + hs_get.error);
        }
        // check response to see if user exists
        else
        {
            // account doesn't exist
            if (hs_get.downloadHandler.text == "false")
            {
                logInValidation.text = "The username/password does not exist.";
            }
            else
            {
                logInValidation.text = "Signing in.";

                // set username and password for user object
                user.username = username;
                user.password = password;

                // start lab scene
                PlayGameUser();
            }
        }

        checkingUser = false;
    }
}
