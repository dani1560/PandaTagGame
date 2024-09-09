using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinChanger : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer[] renderers;
    [SerializeField] Material[] materials;
    [SerializeField] GameObject[] hats;
    public bool isCustomizing;
    private void Start()
    {
        ApplyCustomization();
    }
    private void OnEnable()
    {
        ApplyCustomization();
    }
    public void ApplyCustomization()
    {
        SetMaterial(GameManager.Instance.skinSelected);
        SetHat(GameManager.Instance.hatSelected);
    }

    public void SetMaterial(int no)
    {
        foreach (var renderer in renderers)
        {
            if(!isCustomizing)
            {
                renderer.material = materials[PlayerPrefs.GetInt("SelectedMat")];
            }
            else
            {
                renderer.material = materials[no];
            }
            
        }
    }

    public void SetHat(int no)
    {
        Debug.Log("Setting Hat : " +no+" Player Prefs Value :"+PlayerPrefs.GetInt("SelectedHat"));

        foreach (GameObject g in hats) g.SetActive(false);
        if(!isCustomizing)
        {
            hats[PlayerPrefs.GetInt("SelectedHat")].SetActive(true);
        }
        else
        {
            hats[no].SetActive(true);
        }

        
    }

    public int GetHatsLength()
    {
        return hats.Length;
    }

    public int GetMaterialsLength()
    {
        return materials.Length;
    }
}
