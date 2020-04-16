﻿/*
 * This class defines the materails that refract the 
 * laser with their index of refraction.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// model for serializing a list of refractable materials to JSON
[Serializable]
public class RefractableMaterials
{
    [SerializeField]
    public List<RefractableMaterialModel> materials;

    public RefractableMaterials()
    {
        materials = new List<RefractableMaterialModel>();
    }
}

// model for serializing refractable materials to JSON
[Serializable]
public class RefractableMaterialModel
{
    public RefractableMaterialModel(int pr, int shape,float n, Vector3 pos, Quaternion rot, Vector3 scale, bool reflectable)
    {
        presetRefraction = pr;
        this.shape = shape;
        this.n = n;
        this.pos = pos;
        this.rot = rot;
        this.scale = scale;
        this.reflectable = reflectable;
    }

    [SerializeField]
    public int shape;

    [SerializeField]
    public int presetRefraction;

    [SerializeField]
    public float n;

    [SerializeField]
    public bool reflectable;

    [SerializeField]
    public Vector3 pos;

    [SerializeField]
    public Quaternion rot;
    [SerializeField]
    public Vector3 scale;
}

public class RefractableMaterial : MonoBehaviour
{
    public enum IndexesOfRefraction
    {
        VACUUM,
        AIR,
        HELIUM,
        HYDROGEN,
        CARBON_DIOXIDE,
        WATER,
        ETHANOL,
        OLIVE_OIL,
        ICE,
        FUSED_SILICA,
        PMMA,
        WINDOW_GLASS,
        POLYCARBONATE,
        FLINT_GLASS,
        SAPPHIRE,
        CUBIC_ZIRCONIA,
        DIAMOND,
        MOISSANITE,
        CUSTOM,
        INDEXES_OF_REFRACTION
    };

    public enum Shapes 
    {
        CUBE,
        SPHERE,
        SHAPES
    }

    public Shapes currentShape;

    public Text text;                               // displays refracted angle in the lab scene
    
    private IndexesOfRefraction presetRefraction;   // current preset refraction

    private MeshFilter filter;
    [SerializeField]
    private Mesh cube;
    [SerializeField]
    private Mesh sphere;

    private Collider currentCollider;
    private BoxCollider bCollider;
    private SphereCollider sCollider;
    
    private float n;                                // index of refraction

    private bool reflectable;

    // Start is called before the first frame update
    void Awake()
    {
        presetRefraction = IndexesOfRefraction.WINDOW_GLASS;

        filter = GetComponent<MeshFilter>();

        bCollider = GetComponent<BoxCollider>();
        sCollider = GetComponent<SphereCollider>();

        currentCollider = bCollider;

        SetShape(Shapes.SPHERE);

        n = 1.52f;
    }

    public void SetShape(Shapes shape)
    {
        if (sCollider == null && bCollider == null)
            return;
        
        currentCollider.enabled = false;

        switch(shape)
        {
            case Shapes.CUBE:
                filter.mesh = cube;
                bCollider.enabled = true;
                break;
            case Shapes.SPHERE:
                filter.mesh = sphere;
                sCollider.enabled = true;
                break;
        }

        currentShape = shape;
    }

    public void SetPresetRefraction(IndexesOfRefraction preset)
    {
        presetRefraction = preset;
    }

    public void SetCustomRefraction(float n)
    {
        this.n = n;
    }

    public void SetPos(Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetRot(Quaternion rot)
    {
        transform.rotation = rot;
    }

    public void SetReflectable(bool reflectable)
    {
        this.reflectable = reflectable;
    }

    public int GetShape()
    {
        return (int)currentShape;
    }

    public int GetPresetIndex()
    {
        return (int)presetRefraction;
    }

    public float GetIndexOfRefraction()
    { 
        switch (presetRefraction)
        {
            case IndexesOfRefraction.VACUUM:
                return 1.0f;
            case IndexesOfRefraction.AIR:
                return 1.000293f;
            case IndexesOfRefraction.HELIUM:
                return 1.000036f;
            case IndexesOfRefraction.HYDROGEN:
                return 1.000132f;
            case IndexesOfRefraction.CARBON_DIOXIDE:
                return 1.00045f;
            case IndexesOfRefraction.WATER:
                return 1.333f;
            case IndexesOfRefraction.ETHANOL:
                return 1.36f;
            case IndexesOfRefraction.OLIVE_OIL:
                return 1.47f;
            case IndexesOfRefraction.ICE:
                return 1.31f;
            case IndexesOfRefraction.FUSED_SILICA:
                return 1.46f;
            case IndexesOfRefraction.PMMA:
                return 1.49f;
            case IndexesOfRefraction.WINDOW_GLASS:
                return 1.52f;
            case IndexesOfRefraction.POLYCARBONATE:
                return 1.58f;
            case IndexesOfRefraction.FLINT_GLASS:
                return 1.62f;
            case IndexesOfRefraction.SAPPHIRE:
                return 1.77f;
            case IndexesOfRefraction.CUBIC_ZIRCONIA:
                return 2.15f;
            case IndexesOfRefraction.DIAMOND:
                return 2.42f;
            case IndexesOfRefraction.MOISSANITE:
                return 2.65f;
            default:
                return n;
        }
    }

    public float GetN()
    {
        return n;
    }

    public bool IsReflectable()
    {
        return reflectable;
    }
}
