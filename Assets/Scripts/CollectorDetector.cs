using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectorDetector : MonoBehaviour {

    public List<Collector> collectors = new List<Collector>();
    public GameObject Camera;
    public AudioSource audioSource;
    public float minInterval = 1f;
    public float maxInterval = 1f;
    private float timer = 0f;
    public float minSound = 0.2f;
    public float interval = 0f;
    public List<AudioClip> audioClips = new List<AudioClip>();
    public bool isPlaying = false;

    void Update() {
        if (!isPlaying) { return; }

        collectors = collectors.Where(collector => !collector.isTriggered).ToList();
        if (collectors.Count == 0) {
            return;
        }

        Collector closestCollector = collectors.OrderBy(collector => Vector3.Distance(transform.position, collector.transform.position)).FirstOrDefault();

        Vector3 direction = (closestCollector.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, closestCollector.transform.position);
        
        audioSource.transform.position = Camera.transform.position + direction * 2f;
        audioSource.volume = Mathf.Max(minSound, Mathf.Clamp01(1f - (distance / 100f)));

        timer += Time.deltaTime;
        if (timer >= interval) {
            interval = Mathf.Lerp(minInterval, maxInterval, audioSource.volume);
            audioSource.clip = audioClips[Random.Range(0, audioClips.Count)];
            audioSource.Play();
            timer = 0f;
        }
    }
}