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
    public Text shapeText;
    public Text objsText;
    public Text scaleSliderText;
    public Text rText;
    public Text gText;
    public Text bText;

     
    public InputField angleText;                // input field for object's angle
    public InputField refractionField;          // input field for object's index of refraction
    public InputField worldRefractionField;     // input field for the environments index of refraction

    public Slider angleSlider;                  // slider for changing object's angle
    public Slider scaleSlider;                  // slider for changing object's scale
    public Slider rSlider;                      // slider for changing laser's red color value
    public Slider gSlider;                      // slider for changing laser's green color value
    public Slider bSlider;                      // slider for changing laser's blue color value

    public Toggle reflectable;                  // toggle for enabling and disabling reflectability on object
    public Toggle laserEnabled;                 // toggle for enabling and disabling laser object

    public Dropdown indexesMenu;                // drop down menu filled preset indexes of refraction for objects
    public Dropdown worldIndexesMenu;           // drop down menu filled preset indexes of refraction for the environment
    public Dropdown shapesMenu;                 // drop down menu filled preset shapes for objects

    private List<string> indexesOfRefraction;
    private List<string> shapes;

    private LabManager lm;                      // instance of lab manager
    
    private bool draggingAngleSlider;           // bool to check if angle slider is being dragged
    private bool editingAngleField;             // bool to check if angle field is being edited

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

        var shapes = Enum.GetValues(typeof(RefractableMaterial.Shapes));

        this.shapes = new List<string>();

        foreach (RefractableMaterial.Shapes index in shapes)
        {
            this.shapes.Add(index.ToString());
        }

        this.shapes.RemoveAt(this.shapes.Count - 1);

        indexesMenu.AddOptions(this.indexesOfRefraction);
        worldIndexesMenu.AddOptions(this.indexesOfRefraction);
        shapesMenu.AddOptions(this.shapes);
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
        LaserController lc = lm.obj.GetComponent<LaserController>();

        if (lm.ObjHasChanged())
        {
            laserEnabled.isOn = lc.enabled;
            rSlider.value = lc.color.r;
            gSlider.value = lc.color.g;
            bSlider.value = lc.color.b;

            lm.HasObjChanged(false);
        }

        ShowToggle(reflectable, false);
        ShowToggle(laserEnabled, true);
        ShowIndexComponents(false);
        ShowShapeComponents(false);

        rSlider.gameObject.SetActive(true);
        gSlider.gameObject.SetActive(true);
        bSlider.gameObject.SetActive(true);

        rText.gameObject.SetActive(true);
        gText.gameObject.SetActive(true);
        bText.gameObject.SetActive(true);
        
        scaleSlider.gameObject.SetActive(false);
        scaleSliderText.gameObject.SetActive(false);

        rText.text = "Red: " + rSlider.value;
        gText.text = "Green: " + gSlider.value;
        bText.text = "Blue: " + bSlider.value;

        lc.color.r = rSlider.value;
        lc.color.g = gSlider.value;
        lc.color.b = bSlider.value;

        objText.text = "Laser";
    }

    /*
     * enable UI components for refractable material objects
     */
    public void RunRMUIComponents()
    {
        RefractableMaterial rm = lm.obj.GetComponent<RefractableMaterial>();

        if (lm.ObjHasChanged())
        {
            reflectable.isOn = rm.IsReflectable();
            scaleSlider.value = rm.transform.localScale.x;
            
            lm.HasObjChanged(false);
            
            shapesMenu.value = rm.GetShape();
        }

        ShowToggle(reflectable, true);
        ShowToggle(laserEnabled, false);
        ShowIndexComponents(true);
        ShowShapeComponents(true);

        rSlider.gameObject.SetActive(false);
        gSlider.gameObject.SetActive(false);
        bSlider.gameObject.SetActive(false);

        rText.gameObject.SetActive(false);
        gText.gameObject.SetActive(false);
        bText.gameObject.SetActive(false);

        scaleSlider.gameObject.SetActive(true);
        scaleSliderText.gameObject.SetActive(true);

        scaleSliderText.text = "Scale: " + scaleSlider.value;

        rm.transform.localScale = Vector3.one * scaleSlider.value;

        objText.text = "Refractable Material";

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
    public void ShowToggle(Toggle toggle, bool set)
    {
        toggle.enabled = set;
        
        foreach (Image image in toggle.GetComponentsInChildren<Image>())
        {
            image.enabled = set;
        }

        toggle.GetComponentInChildren<Text>().enabled = set;
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
     * show shape UI components:
     * -text
     * -dropdown menu
     */
    public void ShowShapeComponents(bool set)
    {
        shapeText.enabled = set;

        shapesMenu.enabled = set;
        shapesMenu.image.enabled = set;
        foreach (Image image in shapesMenu.GetComponentsInChildren<Image>())
        {
            image.enabled = set;
        }
        shapesMenu.GetComponentInChildren<Text>().enabled = set;
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
        LaserController lc = lm.obj.GetComponent<LaserController>();

        if (lc != null && !lm.ObjHasChanged())
        {
            lc.enabled = laserEnabled.isOn;
            lc.GetComponent<LineRenderer>().enabled = laserEnabled.isOn;
        }
    }

    /*
     * enables/disables laser components when toggle button is clicked
     */
    public void ReflectableToggleChange()
    {
        RefractableMaterial rm = lm.obj.GetComponent<RefractableMaterial>();

        if (rm != null && !lm.ObjHasChanged())
        {
            rm.SetReflectable(reflectable.isOn);
        }
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
     * when a new shape is set for the environment object
     */
    public void OnShapeIndexChange()
    {
        RefractableMaterial rm = lm.obj.GetComponent<RefractableMaterial>();
        
        rm.SetShape((RefractableMaterial.Shapes)Enum.ToObject(typeof(RefractableMaterial.Shapes), shapesMenu.value));
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
