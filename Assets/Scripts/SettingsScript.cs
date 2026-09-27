using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private AudioMixer AM;
    [SerializeField] private Slider MasterSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider BGMSlider;

    [SerializeField] private TextMeshProUGUI AutoNotesText;

    public static SettingsScript instance;

    float MasterVol;
    float BGMVol;
    float SFXVol;
    int IsAutoNotesOn;
    public bool IsAutoNotesOnBool;
    public void SetMixer(string param,float value)
    {
        //value is 0 - 1
        AM.SetFloat(param, Mathf.Log10(value) * 20); //if its 0, -80f, if its 1, -20f
    }
    void Start()
    {
        //persistent singleton 
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);


        //get from prefs
        MasterVol = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        BGMVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        SFXVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        IsAutoNotesOn = PlayerPrefs.GetInt("EnableAutoNotes", 1); //1 = yes, 0 = no

        if (IsAutoNotesOn == 1)
        {
            AutoNotesText.text = "Yes";
            IsAutoNotesOnBool = true;
        }
        else
        {
            IsAutoNotesOnBool = false;
            AutoNotesText.text = "Off";
        }

        //set the mixer cause it saves or smth
        SetMixer("Master", MasterVol);
        SetMixer("BGM", BGMVol);
        SetMixer("SFX", SFXVol);

        //set slider to it
        MasterSlider.value = Mathf.Pow(10, MasterVol) / 20;
        SFXSlider.value = Mathf.Pow(10, SFXVol)/ 20;
        BGMSlider.value = Mathf.Pow(10, BGMVol)/ 20;

        MasterSlider.onValueChanged.AddListener((float val) =>
        {
            SetMixer("Master", val);
            PlayerPrefs.SetFloat("MasterVolume", val);
        });

        SFXSlider.onValueChanged.AddListener((float val) =>
        {
            SetMixer("SFX", val);
            PlayerPrefs.SetFloat("SFXVolume", val);
        });

        BGMSlider.onValueChanged.AddListener((float val) =>
        {
            SetMixer("BGM", val);
            PlayerPrefs.SetFloat("BGMVolume", val);
        });
        gameObject.SetActive(false);
        gameObject.SetActive(true); //done loading it all, make it active in case
    }

    void OnDisable()
    {
        BGMSlider.onValueChanged.RemoveAllListeners();
        SFXSlider.onValueChanged.RemoveAllListeners();
        MasterSlider.onValueChanged.RemoveAllListeners();
    }

    void OnEnable()
    {
        MasterSlider.onValueChanged.AddListener((float val) =>
        {
            SetMixer("Master", val);
            PlayerPrefs.SetFloat("MasterVolume", val);
        });

        SFXSlider.onValueChanged.AddListener((float val) =>
        {
            SetMixer("SFX", val);
            PlayerPrefs.SetFloat("SFXVolume", val);
        });

        BGMSlider.onValueChanged.AddListener((float val) =>
        {
            SetMixer("BGM", val);
            PlayerPrefs.SetFloat("BGMVolume", val);
        });
    }

    public void AutoNotesButtonPressed()
    {
        if (IsAutoNotesOn == 1)
        {
            IsAutoNotesOn = 0;
            IsAutoNotesOnBool = false;
            AutoNotesText.text = "Off";
        }
        else
        {
            IsAutoNotesOn = 1;
            IsAutoNotesOnBool = true;
            AutoNotesText.text = "On";
        }
        PlayerPrefs.SetInt("EnableAutoNotes", IsAutoNotesOn);
    }
}
