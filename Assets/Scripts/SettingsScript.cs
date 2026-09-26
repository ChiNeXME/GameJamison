using System;
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

    float MasterVol;
    float BGMVol;
    float SFXVol;
    public void SetMixer(string param,float value)
    {
        //value is 0 - 1
        AM.SetFloat(param, Mathf.Log10(value) * 20); //if its 0, -80f, if its 1, -20f
    }
    void Start()
    {
        //get from prefs
        MasterVol = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        BGMVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        SFXVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        //set the mixer cause it saves or smth
        SetMixer("Master", MasterVol);
        SetMixer("BGM", BGMVol);
        SetMixer("SFX", SFXVol);

        //set slider to it
        Debug.Log(MasterSlider.value);
        MasterSlider.value = Mathf.Pow(10, MasterVol) / 20;
        Debug.Log(Mathf.Pow(10, MasterVol)/20);
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
}
