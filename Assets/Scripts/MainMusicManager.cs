using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMusicManager : MonoBehaviour {
    public static MainMusicManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public List<AudioClip> mainMusic = new List<AudioClip>();
    public int currentMusicIndex = 0;
    public float crossfadeDuration = 5f;
    public float maxVolume = .6f;
    
    private bool isUsingSource1 = true;
    
    public void NextMusic()
    {
        if(currentMusicIndex == 0){
            audioSource1.volume = maxVolume;
            audioSource1.clip = mainMusic[currentMusicIndex];
            audioSource1.Play();
            return;
        }
        StartCoroutine(CrossfadeToNextMusic());
    }
    
    private IEnumerator CrossfadeToNextMusic()
    {
        AudioSource currentSource = isUsingSource1 ? audioSource1 : audioSource2;
        AudioSource nextSource = isUsingSource1 ? audioSource2 : audioSource1;
        
        nextSource.clip = mainMusic[currentMusicIndex];
        nextSource.volume = 0f;
        nextSource.Play();
        
        float elapsedTime = 0f;
        
        while (elapsedTime < crossfadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / crossfadeDuration;
            
            currentSource.volume = Mathf.Lerp(maxVolume, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, maxVolume, t);
            
            yield return null;
        }
        
        currentSource.Stop();
        currentSource.volume = maxVolume;
        nextSource.volume = maxVolume;
        
        isUsingSource1 = !isUsingSource1;
        currentMusicIndex++;
    }
}