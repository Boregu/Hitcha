using UnityEngine;

public class SoundEffectManager : MonoBehaviour {
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundEffect;
    
    [Header("Pitch Randomization")]
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    [Header("Trigger Settings")]
    [SerializeField] private KeyCode triggerKey = KeyCode.Space; // Change this to the key you want

    private void Start() {
        if (audioSource == null) {
            Debug.LogWarning("AudioSource is not assigned! Assign an AudioSource in the Inspector.");
        }
    }

    private void Update() {
        if (Input.GetKeyDown(triggerKey)) {
            PlaySound();
        }
    }

    public void PlaySound() {
        if (audioSource == null || soundEffect == null) return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(soundEffect);
    }
}
