using UnityEngine.Audio;
using UnityEngine;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public void SetMasterVolume(float level){
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(level) * 20);
    }

    public void SetMusicVolume(float level){
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(level) * 20);
    }

    public void SetFXVolume(float level){
        audioMixer.SetFloat("FXVolume", Mathf.Log10(level) * 20);
    }
    
    public void SetAmbientVolume(float level){
        audioMixer.SetFloat("AmbientVolume", Mathf.Log10(level) * 20);
    }
}
