/*
 * This class handles the UI componenets of the lab scene, as well as its
 * interactions with objects in the scene.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LabUI : MonoBehaviour
{
    public Text objText;
    public Text positionText;
    public Text refractionText;
    public Text objsText;
     
    public InputField angleText;
    public InputField refractionField;
    public InputField worldRefractionField;

    public Slider angleSlider;

    public Toggle laserEnabled;

    public Dropdown indexesMenu;
    public Dropdown worldIndexesMenu;

    private List<string> indexesOfRefraction;

    private LabManager lm;
    
    private bool draggingAngleSlider;
    private bool editingAngleField;

    // Start is called before the first frame update
    void Start()
    {
        lm = FindObjectOfType<LabManager>();

        draggingAngleSlider = false;
        editingAngleField = false;

        foreach (Text text in refractionField.GetComponentsInChildren<Text>())
        {
            text.enabled = false;
        }

        var indexesOfRefraction = Enum.GetValues(typeof(RefractableMaterial.IndexesOfRefraction));

        this.indexesOfRefraction = new List<string>();

        foreach (RefractableMaterial.IndexesOfRefraction index in indexesOfRefraction)
        {
            this.indexesOfRefraction.Add(index.ToString());
        }

        this.indexesOfRefraction.RemoveAt(this.indexesOfRefraction.Count - 1);

        indexesMenu.AddOptions(this.indexesOfRefraction);
        worldIndexesMenu.AddOptions(this.indexesOfRefraction);
    }

    // Update is called once per frame
    void Update()
    {
        // laser object is currently selected
        if (lm.obj.GetComponent<LaserController>() != null)
        {
            RunLaserUIComponents();
        }
        // refractable material is currently selected
        else
        {
            RunRMUIComponents();
        }

        RunSelectedObjectUIComponents();

        RunLabUIComponents();

        if (Input.GetMouseButtonUp(0))
        {
            draggingAngleSlider = false;
        }
    }

    /*
     * enable UI components for the laser object
     */
    public void RunLaserUIComponents()
    {
        ShowToggle(true);
        ShowIndexComponents(false);

        objText.text = "Laser";
    }

    /*
     * enable UI components for refractable material objects
     */
    public void RunRMUIComponents()
    {
        ShowToggle(false);
        ShowIndexComponents(true);

        objText.text = "Refractable Material";

        RefractableMaterial rm = lm.obj.GetComponent<RefractableMaterial>();

        indexesMenu.value = rm.GetPresetIndex();

        if (indexesMenu.value != (int)RefractableMaterial.IndexesOfRefraction.CUSTOM)
        {
            refractionField.text = "" + rm.GetIndexOfRefraction();
            refractionField.enabled = false;
        }
        else
        {
            refractionField.enabled = true;
        }
    }

    public void RunSelectedObjectUIComponents()
    {
        positionText.text = "Position: " + lm.obj.transform.position;

        // user is not entering the angle manually
        if (!editingAngleField)
        {
            // editing angle through slider
            if (draggingAngleSlider)
            {
                // set object's rotation to angle slider
                lm.obj.transform.rotation = Quaternion.Euler(new Vector3(0.0f, angleSlider.value, 0.0f));
            }
            else
            {
                // set angle slider value the selected object's euler angle y axis
                angleSlider.value = lm.obj.transform.rotation.eulerAngles.y;
            }

            angleText.text = String.Format("{0:0.00}", angleSlider.value);
        }
    }

    /*
     * handles lab environment UI components
     */
    private void RunLabUIComponents()
    {
        worldIndexesMenu.value = lm.rm.GetPresetIndex();

        if (worldIndexesMenu.value != (int)RefractableMaterial.IndexesOfRefraction.CUSTOM)
        {
            worldRefractionField.text = "" + lm.rm.GetIndexOfRefraction();
            worldRefractionField.enabled = false;
        }
        else
        {
            worldRefractionField.enabled = true;
        }

        objsText.text = "" + lm.ObjectCount();
    }

    /*
     * show enable laser toggle button
     */
    public void ShowToggle(bool set)
    {
        laserEnabled.enabled = set;
        
        foreach (Image image in laserEnabled.GetComponentsInChildren<Image>())
        {
            image.enabled = set;
        }

        laserEnabled.GetComponentInChildren<Text>().enabled = set;
    }

    /*
     * show index of refraction UI components:
     * -text
     * -input field
     * -dropdown menu
     */
    public void ShowIndexComponents(bool set)
    {
        refractionText.enabled = set;

        refractionField.enabled = set;
        refractionField.image.enabled = set;

        refractionField.GetComponentInChildren<Text>().enabled = set;

        indexesMenu.enabled = set;
        indexesMenu.image.enabled = set;
        foreach (Image image in indexesMenu.GetComponentsInChildren<Image>())
        {
            image.enabled = set;
        }
        indexesMenu.GetComponentInChildren<Text>().enabled = set;
    }

    /*
     * checks input in input field
     */
    public float CheckFieldInputs(string input, float min, float max)
    {
        try
        {
            // clamp input
            return (Mathf.Clamp(float.Parse(input), min, max));
        }
        catch
        {
            // if input returns an exception set field as 0.0f
            return 0.0f;
        }
    }

    /*
     * when angle slider is being dragged
     */
    public void OnDragAngleSlider()
    {
        draggingAngleSlider = true;
    }

    /*
     * when angle input field is selected
     */
    public void OnSelectAngleInput()
    {
        editingAngleField = true;
    }

    /*
     * when angle in input field is submitted
     */
    public void OnSubmitAngle()
    {
        float newAngle = CheckFieldInputs(angleText.text, 0.0f, 360.0f);

        lm.obj.transform.rotation = Quaternion.Euler(0.0f, newAngle, 0.0f);

        angleSlider.value = lm.obj.transform.rotation.eulerAngles.y;

        editingAngleField = false;
    }
    
    /*
     * enables/disables laser components when toggle button is clicked
     */
    public void ToggleChange()
    {
        lm.obj.GetComponent<LaserController>().enabled = laserEnabled.isOn;
        lm.obj.GetComponent<LineRenderer>().enabled = laserEnabled.isOn;
    }

    /*
     * when a new preset index of refraction is set for a selected object
     */
    public void OnIndexChange()
    {
        RefractableMaterial rm = lm.obj.GetComponent<RefractableMaterial>();

        rm.SetPresetRefraction((RefractableMaterial.IndexesOfRefraction)Enum.ToObject(typeof(RefractableMaterial.IndexesOfRefraction), indexesMenu.value));
    }

    /*
     * when a new preset index of refraction is set for the environment object
     */
    public void OnWorldIndexChange()
    {
        lm.rm.SetPresetRefraction((RefractableMaterial.IndexesOfRefraction)Enum.ToObject(typeof(RefractableMaterial.IndexesOfRefraction), worldIndexesMenu.value));
    }

    /*
     * when a new custom index of refraction is entered for a selected object
     */
    public void OnSubmitIndex()
    {
        refractionField.text = NewIndex(lm.obj.GetComponent<RefractableMaterial>(), refractionField.text);
    }

    /*
     * when a new custom index of refraction is entered for the environment object
     */
    public void OnSubmitWorldIndex()
    {
        worldRefractionField.text = NewIndex(lm.rm, worldRefractionField.text);
    }

    /*
     * when a new custom index of refraction is entered
     */
    public string NewIndex(RefractableMaterial rm, string input)
    {
        float newRefraction = CheckFieldInputs(input, 0.0f, 10.0f);

        rm.SetCustomRefraction(newRefraction);

        return "" + rm.GetIndexOfRefraction();
    }

    public void RemoveObject()
    {
        lm.RemoveObject();
    }

    public void AddObject()
    {
        lm.AddObject();
    }
}
