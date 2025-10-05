// WindAudioController.cs
// Attach to your plane GameObject. Requires an AudioMixer or per-source volume/pitch control.
// This script crossfades idle and movement wind loops based on speed, with optional stall whistle.

// Usage:
// - Create 3 AudioSources as children: "WindIdle", "WindMove", "StallWhistle" (stall optional).
// - Assign clips: seamless loop for WindIdle/WindMove; short loop or one-shot for StallWhistle.
// - Set WindIdle to loop, low volume; WindMove to loop, starts silent; StallWhistle can loop or one-shot.
// - Drag this script onto the plane and assign references in the Inspector.

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WindAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource windIdle;       // soft ambient breeze when not moving
    [SerializeField] private AudioSource windMove;       // whoosh that scales with speed
    [SerializeField] private AudioSource stallWhistle;   // optional: tone when near stall

    [Header("Speed Inputs")]
    [SerializeField] private Rigidbody rb;               // plane rigidbody
    [SerializeField] private bool useRigidbodyVelocity = true;
    [SerializeField] private float manualSpeed;          // if not using rb, you can set this externally (m/s)

    [Header("Tuning")]
    [Tooltip("Speed (m/s) where movement wind begins to fade in.")]
    [SerializeField] private float moveStartSpeed = 5f;
    [Tooltip("Speed (m/s) where movement wind reaches full intensity.")]
    [SerializeField] private float moveFullSpeed = 45f;
    [Tooltip("Overall volume scaling for wind sources.")]
    [SerializeField] private float masterVolume = 1f;
    [Tooltip("Smoothing time (seconds) for volume transitions.")]
    [SerializeField] private float volumeSmoothTime = 0.2f;
    [Tooltip("Pitch range for movement wind based on speed.")]
    [SerializeField] private Vector2 movePitchRange = new Vector2(0.9f, 1.25f);
    [Tooltip("Pitch range for idle wind based on speed (subtle).")]
    [SerializeField] private Vector2 idlePitchRange = new Vector2(1.0f, 1.05f);

    [Header("Stall Whistle")]
    [Tooltip("Enable stall whistle feedback.")]
    [SerializeField] private bool enableStallWhistle = true;
    [Tooltip("Angle of attack (deg) or lift coefficient surrogate to trigger stall zone.")]
    [SerializeField] private float stallAoAThreshold = 14f;
    [Tooltip("Fade in speed for stall whistle intensity (0–1).")]
    [SerializeField] private float stallIntensitySmoothTime = 0.25f;
    [Tooltip("Multiplier applied to stall whistle volume.")]
    [SerializeField] private float stallVolume = 0.35f;

    // If you don't have AoA, you can supply it externally via SetAoA.
    private float currentAoA; // degrees

    // Smooth-damp helpers
    private float idleVolVel, moveVolVel, stallVolVel;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // Ensure audio sources are set and looping for ambience
        ConfigureLoop(windIdle, true);
        ConfigureLoop(windMove, true);
        if (stallWhistle) ConfigureLoop(stallWhistle, true);

        // Start playback so we only adjust volumes/pitches
        SafePlay(windIdle);
        SafePlay(windMove);
        if (enableStallWhistle && stallWhistle) SafePlay(stallWhistle);
    }

    private void LateUpdate()
    {
        float speed = useRigidbodyVelocity ? rb.linearVelocity.magnitude : manualSpeed;
        // Map speed to a 0–1 intensity where 0 = idle, 1 = full movement wind
        float moveIntensity = Mathf.InverseLerp(moveStartSpeed, moveFullSpeed, speed);
        moveIntensity = Mathf.Clamp01(moveIntensity);
        // Idle intensity is the complement; keep a floor so idle doesn't vanish completely
        float idleTarget = Mathf.Lerp(0.6f, 0.0f, moveIntensity); // 0.6 at stop, fades to 0 by full speed
        float moveTarget = moveIntensity;                          // scales 0→1 with speed

        // Smooth volumes
        float idleVol = SmoothClamp(windIdle.volume, idleTarget * masterVolume, ref idleVolVel, volumeSmoothTime);
        float moveVol = SmoothClamp(windMove.volume, moveTarget * masterVolume, ref moveVolVel, volumeSmoothTime);

        windIdle.volume = idleVol;
        windMove.volume = moveVol;

        // Pitch reacts to speed for extra motion feel
        windMove.pitch = Mathf.Lerp(movePitchRange.x, movePitchRange.y, moveIntensity);
        // Idle pitch barely changes to avoid drawing attention
        windIdle.pitch = Mathf.Lerp(idlePitchRange.x, idlePitchRange.y, moveIntensity);

        // Stall whistle based on AoA (optional)
        if (enableStallWhistle && stallWhistle)
        {
            // Simple AoA-to-intensity mapping: below threshold -> 0, above -> ramp up
            float stallTarget = Mathf.InverseLerp(stallAoAThreshold, stallAoAThreshold + 8f, Mathf.Abs(currentAoA));
            float stallVolTarget = stallTarget * stallVolume * masterVolume;
            float stallVol = SmoothClamp(stallWhistle.volume, stallVolTarget, ref stallVolVel, stallIntensitySmoothTime);
            stallWhistle.volume = stallVol;

            // Optionally pitch up slightly with stall intensity
            stallWhistle.pitch = Mathf.Lerp(0.95f, 1.1f, stallTarget);
        }
    }

    // Utility: external setter if your flight model calculates AoA
    public void SetAoA(float degrees)
    {
        currentAoA = degrees;
    }

    // Utility: external setter if you don't use Rigidbody for speed
    public void SetManualSpeed(float speedMetersPerSecond)
    {
        manualSpeed = speedMetersPerSecond;
    }

    private static void ConfigureLoop(AudioSource src, bool loop)
    {
        if (src == null) return;
        src.loop = loop;
        src.playOnAwake = false; // we call Play() manually to avoid race conditions
        src.spatialBlend = 0f;   // 2D for cockpit ambience; set to 1 for 3D if external camera
        src.dopplerLevel = 0f;   // avoid wobble
    }

    private static void SafePlay(AudioSource src)
    {
        if (src == null) return;
        if (!src.isPlaying) src.Play();
    }

    private static float SmoothClamp(float current, float target, ref float velocity, float smoothTime)
    {
        // Mathf.SmoothDamp enforces gradual change and clamps by maxSpeed internally
        return Mathf.SmoothDamp(current, target, ref velocity, Mathf.Max(0.0001f, smoothTime));
    }
}
