using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        if (lm.obj.GetComponent<LaserController>() != null)
        {
            RunLaserUIComponents();
        }
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

    public void RunLaserUIComponents()
    {
        ShowToggle(true);
        ShowIndexComponents(false);

        objText.text = "Laser";
    }

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

        if (!editingAngleField)
        {
            if (draggingAngleSlider)
            {
                lm.obj.transform.rotation = Quaternion.Euler(new Vector3(0.0f, angleSlider.value, 0.0f));
            }
            else
            {
                angleSlider.value = lm.obj.transform.rotation.eulerAngles.y;
            }

            angleText.text = String.Format("{0:0.00}", angleSlider.value);
        }
    }

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

    public void ShowToggle(bool set)
    {
        laserEnabled.enabled = set;
        
        foreach (Image image in laserEnabled.GetComponentsInChildren<Image>())
        {
            image.enabled = set;
        }

        laserEnabled.GetComponentInChildren<Text>().enabled = set;
    }

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

    public float CheckFieldInputs(string input, float min, float max)
    {
        try
        {
            return (Mathf.Clamp(float.Parse(input), min, max));
        }
        catch
        {
            return 0.0f;
        }
    }

    public void OnDragAngleSlider()
    {
        draggingAngleSlider = true;
    }

    public void OnSelectAngleInput()
    {
        editingAngleField = true;
    }

    public void OnSubmitAngle()
    {
        float newAngle = CheckFieldInputs(angleText.text, 0.0f, 360.0f);

        lm.obj.transform.rotation = Quaternion.Euler(0.0f, newAngle, 0.0f);

        angleSlider.value = lm.obj.transform.rotation.eulerAngles.y;

        editingAngleField = false;
    }

    public void ToggleChange()
    {
        lm.obj.GetComponent<LaserController>().enabled = laserEnabled.isOn;
        lm.obj.GetComponent<LineRenderer>().enabled = laserEnabled.isOn;
    }

    public void OnIndexChange()
    {
        RefractableMaterial rm = lm.obj.GetComponent<RefractableMaterial>();

        rm.SetPresetRefraction((RefractableMaterial.IndexesOfRefraction)Enum.ToObject(typeof(RefractableMaterial.IndexesOfRefraction), indexesMenu.value));
    }

    public void OnWorldIndexChange()
    {
        lm.rm.SetPresetRefraction((RefractableMaterial.IndexesOfRefraction)Enum.ToObject(typeof(RefractableMaterial.IndexesOfRefraction), worldIndexesMenu.value));
    }

    public void OnSubmitIndex()
    {
        refractionField.text = NewIndex(lm.obj.GetComponent<RefractableMaterial>(), refractionField.text);
    }

    public void OnSubmitWorldIndex()
    {
        worldRefractionField.text = NewIndex(lm.rm, worldRefractionField.text);
    }

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
